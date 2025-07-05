# MX.Api.Client

This library provides a comprehensive implementation for creating resilient, authenticated REST API clients as part of the MX API Abstractions approach. Built on top of MX.Api.Abstractions, it offers base classes, interfaces, and utilities for creating API clients with features such as authentication, token management, request execution, and standardized response processing.

## Features

- Support for multiple authentication methods (API Key and Entra ID authentication)
- Automatic token acquisition and caching with thread-safe operations
- Built-in retry policies with exponential backoff and circuit breaker patterns
- Thread-safe REST client management
- Standardized error handling and response processing using ApiResponse<T> model
- Support for API key authentication with resilient handling
- Integration with Microsoft.Extensions.Logging for comprehensive diagnostics

## Installation

```bash
dotnet add package MX.Api.Client
```

## Usage

### Basic Setup

```csharp
// Register the API client services
services.AddApiClient()
    .WithApiKeyAuthentication("your-api-key");

// Or with Entra ID authentication
services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience");

// Configure client options
services.Configure<ApiClientOptions>(options =>
{
    options.BaseUrl = "https://api.example.com";
    options.ApiPathPrefix = "v2";
    options.MaxRetryCount = 3;
});
```

### Creating a Custom API Client

```csharp
// Inherit from BaseApi
public class MyApiClient : BaseApi
{
    private readonly ILogger<MyApiClient> logger;

    public MyApiClient(
        ILogger<MyApiClient> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptions<ApiClientOptions> options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
        this.logger = logger;
    }

    // Implement custom API methods
    public async Task<ApiResponse<ResourceDto>> GetResourceAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync($"resources/{id}", Method.Get, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            
            return response.ToApiResponse<ResourceDto>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to retrieve resource with ID {ResourceId}", id);
            var errorResponse = new ApiResponse<ResourceDto>(new ApiError("InternalError", "An unexpected error occurred"));
            return new HttpResponseWrapper<ResourceDto>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
```

### Updating API Key at Runtime

```csharp
// Update authentication options
var authOptions = new ApiKeyAuthenticationOptions
{
    ApiKey = "new-api-key",
    HeaderName = "Ocp-Apim-Subscription-Key" // or your custom header name
};
```

## Authentication Methods

### API Key Authentication

Use this when your API requires an API key in a header (like Azure API Management).

```csharp
services.AddApiClient()
    .WithApiKeyAuthentication("your-api-key", "X-API-Key"); // Custom header name
```

### Entra ID Authentication

Use this when your API requires OAuth tokens from Entra ID (formerly Azure AD).

```csharp
services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience");
```

With custom credential options:

```csharp
services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience", options => 
    {
        options.ExcludeManagedIdentityCredential = true;
        // Other DefaultAzureCredentialOptions
    });
```

## Error Handling and Resilience

The library includes built-in retry policies with exponential backoff for transient failures:

```csharp
services.Configure<ApiClientOptions>(options =>
{
    options.MaxRetryCount = 3; // Configure retry count (default is 3)
});
```

You can also customize the retry behavior:

```csharp
// Custom retry policy
services.AddApiClient()
    .WithCustomRetryPolicy(retryCount => 
        Policy
            .Handle<HttpRequestException>()
            .OrResult<RestResponse>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount, 
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + 
                                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000))
            )
    );
```

## Advanced Usage

### Working with Collections and Filtering

```csharp
public async Task<ApiResponse<CollectionModel<ResourceDto>>> GetResourcesAsync(FilterOptions filter, CancellationToken cancellationToken = default)
{
    try
    {
        var request = await CreateRequestAsync("resources", Method.Get, cancellationToken);
        
        // Add filter options to request
        request.AddFilterOptions(filter);
        
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<CollectionModel<ResourceDto>>();
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        logger.LogError(ex, "Failed to retrieve resources");
        var errorResponse = new ApiResponse<CollectionModel<ResourceDto>>(new ApiError("InternalError", "An unexpected error occurred"));
        return new HttpResponseWrapper<CollectionModel<ResourceDto>>(HttpStatusCode.InternalServerError, errorResponse);
    }
}
```
