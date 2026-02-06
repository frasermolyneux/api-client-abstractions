# MX.Api.Client.Testing

Testing utilities for the MX API Abstractions library. Provides in-memory test doubles and helper extensions to enable fast, reliable unit, integration, and UI testing without making actual HTTP calls.

## Installation

```bash
dotnet add package MX.Api.Client.Testing
```

## What's Included

### Test Doubles

- **`InMemoryRestClientService`** - In-memory REST client that returns predefined responses
  - Configure static responses with `AddResponse()`
  - Configure dynamic responses with `AddResponseFunction()`
  - Track and verify API calls with `WasCalled()` and `WasCalledTimes()`

- **`FakeApiTokenProvider`** - Fake authentication token provider
  - Configure tokens per audience with `SetToken()`
  - Set default tokens with `SetDefaultToken()`
  - Track and verify token requests with `WasTokenRequested()`

### DI Extensions

- **`AddTestApiClient<TInterface, TImplementation>()`** - Register API client with test doubles
- **`AddTestTypedApiClient<...>()`** - Register API client with custom options and test doubles
- **`UseInMemoryRestClientService()`** - Replace existing REST client service with in-memory version
- **`UseFakeApiTokenProvider()`** - Replace existing token provider with fake version

## Quick Start

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
        var client = serviceProvider.GetRequiredService<IUserApiClient>();

        // Act
        var result = await client.GetUserAsync("123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("123", result.Result?.Data?.Id);
        
        // Verify the API was called
        Assert.True(testService.WasCalled("users/123"));
    }
}
```

## Features

### ✅ Unit Testing
Test your service layer without HTTP calls or external dependencies.

### ✅ Integration Testing
Test ASP.NET Core applications with in-memory API clients using `WebApplicationFactory`.

### ✅ UI Testing
Run Playwright or Selenium tests without requiring backend infrastructure.

### ✅ Dynamic Responses
Configure responses that inspect request parameters and return customized data.

### ✅ Authentication Testing
Test authentication flows without real credentials or token services.

### ✅ Error Scenario Testing
Easily test error handling by configuring error responses.

## Documentation

For comprehensive examples and best practices, see:
- **[Complete Testing Guide](../../docs/testing-guide.md)** - Unit, integration, and UI testing examples
- **[MX.Api.Client Documentation](../MX.Api.Client/README.md)** - Main package documentation

## Example: Testing with Authentication

```csharp
[Fact]
public async Task ApiClient_UsesAuthentication_Correctly()
{
    // Arrange
    var services = new ServiceCollection();
    
    // Setup fake token provider
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

## Example: Dynamic Test Responses

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
                    Content = $"{{\"data\": {{\"items\": []}}, " +
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

## Benefits

- **🚀 Fast** - No network calls, tests run in milliseconds
- **💯 Reliable** - No flaky tests due to network issues
- **🔒 Isolated** - Tests don't depend on external services
- **🎯 Focused** - Test your code, not the HTTP infrastructure
- **📦 Lightweight** - Minimal dependencies, works with existing test frameworks

## License

Distributed under the [GNU General Public License v3.0](https://github.com/frasermolyneux/api-client-abstractions/blob/main/LICENSE).
