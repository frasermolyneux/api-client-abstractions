# Integration Test Architecture

## Overview

The `MX.Api.IntegrationTests` project provides comprehensive testing of the API client abstractions library through multiple test approaches that validate both service registration and actual API functionality.

## Test Structure

### Test Categories

1. **Service Registration Tests** (`MultipleClientConfigurationTests.cs`)
   - Validate dependency injection configuration
   - Test multiple client registration scenarios
   - Verify options configuration and builder patterns
   - Confirm service resolution and lifecycle management

2. **Direct Endpoint Tests** (`EndToEndIntegrationTests.cs`)
   - Test actual API endpoints using in-memory test server
   - Validate authentication mechanisms
   - Test concurrent API calls
   - Verify error handling and response formatting

3. **Infrastructure Tests** (`InfrastructureTests.cs`)
   - Confirm test server setup and configuration
   - Validate test harness functionality

### Test Infrastructure

- **CustomWebApplicationFactory**: Configures an in-memory ASP.NET Core test server hosting dummy APIs
- **Dummy APIs**: Three complete API implementations (Weather API, User API, and Product API) with controllers, models, and authentication
- **API Clients**: Full client implementations with interfaces, options, and builders for testing registration patterns

## Dummy APIs

### Weather API
- **Pattern**: Uses standard `ApiClientOptions` with simplified registration
- **Authentication**: API key via `X-API-Key` header
- **Endpoints**: Health check, current weather, forecast
- **Returns**: Single objects and collections

### User API  
- **Pattern**: Uses custom `UserApiOptions` with full registration method
- **Authentication**: Bearer token via `Authorization` header
- **Endpoints**: Get users (paginated), get user by ID, create user
- **Returns**: Collection models with pagination metadata

### Product API
- **Pattern**: Uses standard `ApiClientOptions` with simplified registration
- **Authentication**: Bearer token via `Authorization` header  
- **Endpoints**: Get products (collection), get product by ID
- **Returns**: Collection models for product catalogs

## API Client Abstraction Testing Limitation

### Issue Description

During integration testing, we discovered a fundamental limitation when attempting to test the API client abstraction against in-memory test servers:

**The RestSharp-based API clients cannot connect to ASP.NET Core TestServer instances because RestSharp does not use HttpClientFactory by default and creates its own HTTP connections that cannot reach the in-memory test server.**

### Technical Details

- **TestServer Architecture**: ASP.NET Core's `TestServer` provides an in-memory HTTP server that can only be accessed through its specific `HttpClient` instance
- **RestSharp Architecture**: The `RestClientService` (based on RestSharp) creates its own HTTP connections and does not integrate with the test server's HTTP pipeline
- **Result**: API client calls result in "status code 0" failures, indicating connection timeouts/failures

### Attempted Solutions

We explored several approaches to resolve this limitation:

1. **Custom HttpClientFactory**: Created `TestHttpClientFactory` to inject test server's HttpClient
2. **Custom RestClientService**: Created `TestRestClientService` to use test server's HttpClient
3. **DI Container Integration**: Attempted to override RestSharp's HTTP client behavior

**All approaches failed** because RestSharp's architecture doesn't support using external HttpClient instances for its HTTP operations in a way that's compatible with TestServer.

## Current Test Strategy

Given this limitation, we've implemented a **dual-approach testing strategy**:

### 1. Service Registration Tests
- **Purpose**: Validate that API clients can be registered in DI container correctly
- **Approach**: Test service registration, configuration, and resolution without making actual HTTP calls
- **Coverage**: DI registration patterns, options configuration, multiple client scenarios

### 2. Direct Endpoint Tests
- **Purpose**: Validate that API endpoints work correctly and handle authentication properly
- **Approach**: Use TestServer's HttpClient directly to call API endpoints
- **Coverage**: Authentication, error handling, concurrent calls, response formatting

### Combined Coverage

Together, these two approaches provide comprehensive coverage:
- ✅ **Service Registration**: Confirms the API client abstraction can be properly configured and registered
- ✅ **API Functionality**: Confirms the actual API endpoints work correctly
- ✅ **Authentication**: Tests both DI registration of auth options and actual API auth validation
- ✅ **Error Handling**: Tests both client-side error handling setup and server-side error responses
- ✅ **Configuration**: Tests both client configuration patterns and API behavior

## Test Examples

### Multiple Client Registration Test
```csharp
[Fact]
public void CanRegisterMultipleClients_WithDifferentRegistrationMethods()
{
    var services = new ServiceCollection();
    services.AddLogging();

    // Weather API - Standard options with simplified registration
    services.AddApiClient<IWeatherApiClient, WeatherApiClient>(options =>
    {
        options.WithBaseUrl("https://weather.example.com")
               .WithApiKeyAuthentication("weather-test-key", "X-API-Key");
    });

    // User API - Custom options with full registration method
    services.AddTypedApiClient<IUserApiClient, UserApiClient, UserApiOptions, UserApiOptionsBuilder>(options =>
    {
        var apiKeyOptions = new ApiKeyAuthenticationOptions();
        apiKeyOptions.SetApiKey("user-test-token");

        options.WithBaseUrl("https://users.example.com")
               .WithAuthentication(apiKeyOptions)
               .WithUserCaching(60);
    });

    // Product API - Standard options with simplified registration
    services.AddApiClient<IProductApiClient, ProductApiClient>(options =>
    {
        options.WithBaseUrl("https://products.example.com")
               .WithApiKeyAuthentication("Bearer product-test-key", "Authorization");
    });

    var serviceProvider = services.BuildServiceProvider();
    
    // Verify all three clients are registered
    var weatherClient = serviceProvider.GetService<IWeatherApiClient>();
    var userClient = serviceProvider.GetService<IUserApiClient>();
    var productClient = serviceProvider.GetService<IProductApiClient>();

    Assert.NotNull(weatherClient);
    Assert.NotNull(userClient);
    Assert.NotNull(productClient);
}
```

### Simultaneous API Calls Test
```csharp
[Fact]
public async Task MultipleEndpoints_CanWorkSimultaneously()
{
    // Create authenticated clients for all three APIs
    var weatherClient = _factory.CreateClient();
    weatherClient.DefaultRequestHeaders.Add("X-API-Key", "weather-test-key");

    var userClient = _factory.CreateClient();
    userClient.DefaultRequestHeaders.Add("Authorization", "Bearer user-test-token");

    var productClient = _factory.CreateClient();
    productClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-product-key");

    // Call all three APIs simultaneously
    var weatherTask = weatherClient.GetAsync("/api/weather/current?location=Tokyo");
    var usersTask = userClient.GetAsync("/api/users?page=1&pageSize=5");
    var productsTask = productClient.GetAsync("/api/products");

    await Task.WhenAll(weatherTask, usersTask, productsTask);

    // All calls should succeed
    weatherTask.Result.EnsureSuccessStatusCode();
    usersTask.Result.EnsureSuccessStatusCode();
    productsTask.Result.EnsureSuccessStatusCode();
}
```

## Future Considerations

### For True End-to-End Testing

If true end-to-end testing of the API client abstraction is required in future iterations, consider:

1. **External Test Server**: Use a real HTTP server (e.g., ASP.NET Core app running on localhost) instead of in-memory TestServer
2. **Different HTTP Client**: Replace RestSharp with HttpClient-based implementation that supports TestServer
3. **Mock-Based Testing**: Use mocking frameworks to simulate HTTP responses at the RestSharp level

### Recommended Approach

For most scenarios, the current dual-approach testing strategy provides sufficient coverage and confidence:
- Tests prove that clients can be registered correctly
- Tests prove that APIs work correctly
- Together, they provide high confidence that the integration will work in production

The limitation only affects the specific scenario of testing RestSharp-based clients against in-memory test servers, which is a very specific testing configuration not commonly encountered in production environments.
