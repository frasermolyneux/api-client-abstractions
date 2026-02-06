# Testing Guide for API Client Consumers

This guide explains how consuming projects can write comprehensive tests (unit tests, integration tests, and UI tests) using the MX API Abstractions library's built-in testing utilities.

## Table of Contents
- [Overview](#overview)
- [Quick Start](#quick-start)
- [Testing Approaches](#testing-approaches)
- [Unit Testing](#unit-testing)
- [Integration Testing](#integration-testing)
- [UI Testing with Playwright](#ui-testing-with-playwright)
- [Advanced Scenarios](#advanced-scenarios)
- [Best Practices](#best-practices)

## Overview

The MX API Abstractions library provides several test doubles and utilities in the **`MX.Api.Client.Testing`** package that allow you to test your code without making actual HTTP calls:

- **`InMemoryRestClientService`**: An in-memory implementation of `IRestClientService` that returns predefined responses
- **`FakeApiTokenProvider`**: A fake implementation of `IApiTokenProvider` that returns predefined tokens
- **`TestingExtensions`**: Extension methods that simplify setting up test doubles in your DI container

These utilities enable:
- ✅ Fast, reliable tests that don't depend on external APIs
- ✅ Testing error scenarios and edge cases easily
- ✅ Running tests in CI/CD pipelines without network access
- ✅ Testing authentication flows without real credentials
- ✅ UI tests (Playwright, Selenium) that run locally without backend dependencies

## Quick Start

### 1. Install the Testing Package

```bash
dotnet add package MX.Api.Client.Testing
```

### 2. Basic Test Setup

```csharp
using MX.Api.Client.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using RestSharp;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public async Task GetUser_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register API client with test doubles
        var testService = services.AddTestApiClient<IUserApiClient, UserApiClient>(
            options => options.WithBaseUrl("https://test.example.com"),
            testService =>
            {
                testService.AddResponse("users/123", new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"id\": \"123\", \"name\": \"John Doe\"}}"
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var userApiClient = serviceProvider.GetRequiredService<IUserApiClient>();

        // Act
        var result = await userApiClient.GetUserAsync("123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("123", result.Result?.Data?.Id);
        Assert.Equal("John Doe", result.Result?.Data?.Name);
        
        // Verify the API was called
        Assert.True(testService.WasCalled("users/123"));
    }
}
```

## Testing Approaches

### Approach 1: Using AddTestApiClient (Recommended)

The simplest approach for most scenarios. Automatically sets up all test doubles.

```csharp
var testService = services.AddTestApiClient<IMyApiClient, MyApiClient>(
    options => options.WithBaseUrl("https://test.example.com"),
    testService =>
    {
        // Configure predefined responses
        testService.AddResponse("api/endpoint", new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = "{\"data\": \"response\"}"
        });
    });
```

### Approach 2: Replace Existing Registrations

Useful when you have existing service registration and want to swap in test doubles.

```csharp
// Original registration
services.AddApiClient<IMyApiClient, MyApiClient>(
    options => options.WithBaseUrl("https://api.example.com"));

// Replace with test doubles for testing
var testService = services.UseInMemoryRestClientService(testService =>
{
    testService.AddResponse("api/endpoint", new RestResponse
    {
        StatusCode = HttpStatusCode.OK,
        Content = "{\"data\": \"response\"}"
    });
});
```

### Approach 3: Manual Registration

Maximum control for complex scenarios.

```csharp
var inMemoryService = new InMemoryRestClientService();
inMemoryService.AddResponse("api/users", new RestResponse
{
    StatusCode = HttpStatusCode.OK,
    Content = "{\"data\": []}"
});

services.AddSingleton<IRestClientService>(inMemoryService);
services.AddApiClient<IMyApiClient, MyApiClient>(
    options => options.WithBaseUrl("https://test.example.com"));
```

## Unit Testing

### Testing Service Layer

```csharp
public class UserService
{
    private readonly IUserApiClient _userApiClient;

    public UserService(IUserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public async Task<UserDto?> GetActiveUserAsync(string userId)
    {
        var result = await _userApiClient.GetUserAsync(userId);
        
        if (!result.IsSuccess)
            return null;
            
        var user = result.Result?.Data;
        return user?.IsActive == true ? user : null;
    }
}

public class UserServiceTests
{
    [Fact]
    public async Task GetActiveUser_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = services.AddTestApiClient<IUserApiClient, UserApiClient>(
            options => options.WithBaseUrl("https://test.example.com"),
            testService =>
            {
                testService.AddResponse("users/999", new RestResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = "{\"errors\": [{\"code\": \"NOT_FOUND\"}]}"
                });
            });

        services.AddTransient<UserService>();
        var serviceProvider = services.BuildServiceProvider();
        var userService = serviceProvider.GetRequiredService<UserService>();

        // Act
        var user = await userService.GetActiveUserAsync("999");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task GetActiveUser_ReturnsNull_WhenUserIsInactive()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTestApiClient<IUserApiClient, UserApiClient>(
            options => options.WithBaseUrl("https://test.example.com"),
            testService =>
            {
                testService.AddResponse("users/123", new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"id\": \"123\", \"isActive\": false}}"
                });
            });

        services.AddTransient<UserService>();
        var serviceProvider = services.BuildServiceProvider();
        var userService = serviceProvider.GetRequiredService<UserService>();

        // Act
        var user = await userService.GetActiveUserAsync("123");

        // Assert
        Assert.Null(user);
    }
}
```

### Testing Error Handling

```csharp
[Theory]
[InlineData(HttpStatusCode.BadRequest)]
[InlineData(HttpStatusCode.Unauthorized)]
[InlineData(HttpStatusCode.InternalServerError)]
public async Task GetUser_HandlesErrors_Gracefully(HttpStatusCode statusCode)
{
    // Arrange
    var services = new ServiceCollection();
    services.AddTestApiClient<IUserApiClient, UserApiClient>(
        options => options.WithBaseUrl("https://test.example.com"),
        testService =>
        {
            testService.AddResponse("users/123", new RestResponse
            {
                StatusCode = statusCode,
                Content = "{\"errors\": [{\"code\": \"ERROR\"}]}"
            });
        });

    var serviceProvider = services.BuildServiceProvider();
    var client = serviceProvider.GetRequiredService<IUserApiClient>();

    // Act
    var result = await client.GetUserAsync("123");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(statusCode, result.StatusCode);
}
```

### Testing with Dynamic Responses

```csharp
[Fact]
public async Task GetUsers_Pagination_WorksCorrectly()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddTestApiClient<IUserApiClient, UserApiClient>(
        options => options.WithBaseUrl("https://test.example.com"),
        testService =>
        {
            // Use a response function to inspect the request
            testService.AddResponseFunction("users", request =>
            {
                var page = request.Parameters
                    .FirstOrDefault(p => p.Name == "page")?.Value?.ToString() ?? "1";
                var pageSize = request.Parameters
                    .FirstOrDefault(p => p.Name == "pageSize")?.Value?.ToString() ?? "10";

                return new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = $"{{\"data\": {{\"items\": [], \"totalCount\": 100}}, " +
                              $"\"pagination\": {{\"page\": {page}, \"pageSize\": {pageSize}}}}}"
                };
            });
        });

    var serviceProvider = services.BuildServiceProvider();
    var client = serviceProvider.GetRequiredService<IUserApiClient>();

    // Act
    var result = await client.GetUsersAsync(page: 2, pageSize: 20);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Result?.Pagination?.Page);
    Assert.Equal(20, result.Result?.Pagination?.PageSize);
}
```

## Integration Testing

### Testing ASP.NET Core Controllers

```csharp
public class UsersController : ControllerBase
{
    private readonly IUserApiClient _userApiClient;

    public UsersController(IUserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var result = await _userApiClient.GetUserAsync(id);
        return result.ToHttpResult();
    }
}

public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUser_Returns200_WhenUserExists()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the real API client with a test double
                services.AddTestApiClient<IUserApiClient, UserApiClient>(
                    options => options.WithBaseUrl("https://test.example.com"),
                    testService =>
                    {
                        testService.AddResponse("users/123", new RestResponse
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = "{\"data\": {\"id\": \"123\", \"name\": \"John\"}}"
                        });
                    });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/users/123");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("John", content);
    }
}
```

### Testing with Authentication

```csharp
[Fact]
public async Task ApiClient_UsesAuthentication_Correctly()
{
    // Arrange
    var services = new ServiceCollection();
    
    var fakeTokenProvider = new FakeApiTokenProvider();
    fakeTokenProvider.SetToken("api://users", "test-token-123");
    
    services.AddSingleton<IApiTokenProvider>(fakeTokenProvider);
    
    var testService = services.AddTestApiClient<IUserApiClient, UserApiClient>(
        options => options
            .WithBaseUrl("https://test.example.com")
            .WithEntraIdAuthentication("api://users"),
        testService =>
        {
            testService.AddResponse("users/123", new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{\"data\": {\"id\": \"123\"}}"
            });
        });

    var serviceProvider = services.BuildServiceProvider();
    var client = serviceProvider.GetRequiredService<IUserApiClient>();

    // Act
    var result = await client.GetUserAsync("123");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(fakeTokenProvider.WasTokenRequested("api://users"));
}
```

## UI Testing with Playwright

### Setting Up Playwright Tests

```csharp
// PlaywrightTests.cs
public class PlaywrightTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real API clients with test doubles
                    services.AddTestApiClient<IUserApiClient, UserApiClient>(
                        options => options.WithBaseUrl("https://test.example.com"),
                        testService =>
                        {
                            testService.AddResponse("users", new RestResponse
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = "{\"data\": {\"items\": [" +
                                    "{\"id\": \"1\", \"name\": \"Alice\"}," +
                                    "{\"id\": \"2\", \"name\": \"Bob\"}" +
                                    "]}}"
                            });

                            testService.AddResponse("users/1", new RestResponse
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = "{\"data\": {\"id\": \"1\", \"name\": \"Alice\"}}"
                            });
                        });
                });

                // Configure to use a specific URL
                builder.UseUrls("http://localhost:5555");
            });

        await _factory.Services.GetRequiredService<IHostApplicationLifetime>()
            .ApplicationStarted;
    }

    [Fact]
    public async Task UserList_DisplaysUsers()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        
        // Act
        await page.GotoAsync("http://localhost:5555/users");
        await page.WaitForSelectorAsync("text=Alice");

        // Assert
        var content = await page.ContentAsync();
        Assert.Contains("Alice", content);
        Assert.Contains("Bob", content);
    }

    [Fact]
    public async Task UserDetails_ShowsUserInformation()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        
        // Act
        await page.GotoAsync("http://localhost:5555/users/1");
        await page.WaitForSelectorAsync("text=Alice");

        // Assert
        var header = await page.Locator("h1").TextContentAsync();
        Assert.Equal("Alice", header);
    }

    public async Task DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
        await _factory.DisposeAsync();
    }
}
```

### Testing Form Submissions

```csharp
[Fact]
public async Task CreateUser_SubmitsForm_Successfully()
{
    // Arrange
    var testService = new InMemoryRestClientService();
    testService.AddResponseFunction("users", request =>
    {
        // Verify the request body contains expected data
        var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
        
        return new RestResponse
        {
            StatusCode = HttpStatusCode.Created,
            Content = "{\"data\": {\"id\": \"999\", \"name\": \"New User\"}}"
        };
    });

    var factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRestClientService>(testService);
            });
            builder.UseUrls("http://localhost:5555");
        });

    var page = await _browser.NewPageAsync();
    
    // Act
    await page.GotoAsync("http://localhost:5555/users/create");
    await page.FillAsync("input[name='name']", "New User");
    await page.ClickAsync("button[type='submit']");
    await page.WaitForSelectorAsync("text=User created successfully");

    // Assert
    Assert.True(testService.WasCalled("users"));
    var requests = testService.ExecutedRequests;
    Assert.Single(requests);
}
```

## Advanced Scenarios

### Testing Multiple API Clients

```csharp
[Fact]
public async Task MultipleClients_WorkIndependently()
{
    // Arrange
    var services = new ServiceCollection();
    
    var userTestService = services.AddTestApiClient<IUserApiClient, UserApiClient>(
        options => options.WithBaseUrl("https://users.example.com"),
        testService =>
        {
            testService.AddResponse("users/123", new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{\"data\": {\"id\": \"123\"}}"
            });
        });

    var orderTestService = services.AddTestApiClient<IOrderApiClient, OrderApiClient>(
        options => options.WithBaseUrl("https://orders.example.com"),
        testService =>
        {
            testService.AddResponse("orders/456", new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{\"data\": {\"id\": \"456\"}}"
            });
        });

    var serviceProvider = services.BuildServiceProvider();
    var userClient = serviceProvider.GetRequiredService<IUserApiClient>();
    var orderClient = serviceProvider.GetRequiredService<IOrderApiClient>();

    // Act
    var userResult = await userClient.GetUserAsync("123");
    var orderResult = await orderClient.GetOrderAsync("456");

    // Assert
    Assert.True(userResult.IsSuccess);
    Assert.True(orderResult.IsSuccess);
    Assert.True(userTestService.WasCalled("users/123"));
    Assert.True(orderTestService.WasCalled("orders/456"));
}
```

### Testing Retry Behavior

```csharp
[Fact]
public async Task ApiClient_RetriesOnTransientFailure()
{
    // Arrange
    var attemptCount = 0;
    var services = new ServiceCollection();
    
    services.AddTestApiClient<IUserApiClient, UserApiClient>(
        options => options
            .WithBaseUrl("https://test.example.com")
            .WithMaxRetryCount(3),
        testService =>
        {
            testService.AddResponseFunction("users/123", request =>
            {
                attemptCount++;
                if (attemptCount < 3)
                {
                    // Simulate transient failure
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.ServiceUnavailable
                    };
                }
                
                // Success on third attempt
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"id\": \"123\"}}"
                };
            });
        });

    var serviceProvider = services.BuildServiceProvider();
    var client = serviceProvider.GetRequiredService<IUserApiClient>();

    // Act
    var result = await client.GetUserAsync("123");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(3, attemptCount);
}
```

### Verifying Request Details

```csharp
[Fact]
public async Task ApiClient_SendsCorrectHeaders()
{
    // Arrange
    var services = new ServiceCollection();
    var capturedHeaders = new Dictionary<string, string>();
    
    services.AddTestApiClient<IUserApiClient, UserApiClient>(
        options => options
            .WithBaseUrl("https://test.example.com")
            .WithApiKeyAuthentication("my-api-key"),
        testService =>
        {
            testService.AddResponseFunction("users/123", request =>
            {
                // Capture headers for verification
                foreach (var header in request.Parameters.Where(p => p.Type == ParameterType.HttpHeader))
                {
                    capturedHeaders[header.Name!] = header.Value?.ToString() ?? "";
                }
                
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{\"data\": {\"id\": \"123\"}}"
                };
            });
        });

    var serviceProvider = services.BuildServiceProvider();
    var client = serviceProvider.GetRequiredService<IUserApiClient>();

    // Act
    await client.GetUserAsync("123");

    // Assert
    Assert.True(capturedHeaders.ContainsKey("X-API-Key"));
    Assert.Equal("my-api-key", capturedHeaders["X-API-Key"]);
}
```

## Best Practices

### 1. Use Test Doubles for All External Dependencies

```csharp
// ✅ Good: All external dependencies use test doubles
var services = new ServiceCollection();
services.AddTestApiClient<IUserApiClient, UserApiClient>(...);
services.AddTestApiClient<IOrderApiClient, OrderApiClient>(...);
services.AddSingleton<IEmailService, FakeEmailService>();

// ❌ Bad: Mixing real and fake dependencies
services.AddApiClient<IUserApiClient, UserApiClient>(...);  // Real HTTP calls!
services.AddSingleton<IEmailService, FakeEmailService>();
```

### 2. Configure Realistic Responses

```csharp
// ✅ Good: Response mimics real API structure
testService.AddResponse("users/123", new RestResponse
{
    StatusCode = HttpStatusCode.OK,
    Content = @"{
        ""data"": {
            ""id"": ""123"",
            ""name"": ""John Doe"",
            ""email"": ""john@example.com"",
            ""createdAt"": ""2024-01-01T00:00:00Z""
        },
        ""pagination"": null
    }"
});

// ❌ Bad: Minimal response that might miss bugs
testService.AddResponse("users/123", new RestResponse
{
    StatusCode = HttpStatusCode.OK,
    Content = "{\"data\": {}}"
});
```

### 3. Test Both Success and Failure Paths

```csharp
[Theory]
[InlineData(HttpStatusCode.OK, true)]
[InlineData(HttpStatusCode.NotFound, false)]
[InlineData(HttpStatusCode.InternalServerError, false)]
public async Task GetUser_HandlesAllScenarios(HttpStatusCode statusCode, bool shouldSucceed)
{
    // Test setup and assertions...
}
```

### 4. Verify API Calls Were Made

```csharp
// Act
var result = await client.DeleteUserAsync("123");

// Assert
Assert.True(result.IsSuccess);
Assert.True(testService.WasCalled("users/123"));
Assert.True(testService.WasCalledTimes("users/123", 1));
```

### 5. Clean Up Between Tests

```csharp
public class UserServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly InMemoryRestClientService _testService;

    public UserServiceTests()
    {
        var services = new ServiceCollection();
        _testService = services.AddTestApiClient<IUserApiClient, UserApiClient>(...);
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _testService.Clear();
        _serviceProvider.Dispose();
    }
}
```

### 6. Use Descriptive Test Names

```csharp
// ✅ Good: Clear what is being tested
[Fact]
public async Task GetUser_Returns404_WhenUserDoesNotExist() { }

[Fact]
public async Task CreateUser_Returns400_WhenEmailIsInvalid() { }

// ❌ Bad: Unclear test purpose
[Fact]
public async Task Test1() { }

[Fact]
public async Task GetUserTest() { }
```

## Summary

The MX API Abstractions library provides comprehensive testing utilities that enable:

- **Unit Testing**: Test your business logic without HTTP calls
- **Integration Testing**: Test ASP.NET Core applications with in-memory API clients
- **UI Testing**: Run Playwright/Selenium tests without backend dependencies
- **CI/CD Friendly**: Tests run quickly and reliably in any environment

By using `InMemoryRestClientService` and `FakeApiTokenProvider`, you can write fast, reliable tests that give you confidence in your code while maintaining the same API client interfaces used in production.
