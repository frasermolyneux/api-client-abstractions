# Implementation Guide: Consuming APIs with MX API Abstractions

This guide shows how to consume APIs using the MX API Abstractions libraries, whether the target API follows the MX pattern or is a traditional REST API.

## Table of Contents
- [Overview](#overview)
- [Quick Start](#quick-start)
- [Authentication Methods](#authentication-methods)
- [Building API Clients](#building-api-clients)
- [Multiple API Clients](#multiple-api-clients)
- [Error Handling](#error-handling)
- [Advanced Scenarios](#advanced-scenarios)
- [Best Practices](#best-practices)

## Overview

The MX API Abstractions client library enables you to:

- **Build resilient API clients** with automatic retry policies and circuit breakers
- **Handle authentication seamlessly** with support for API keys, Bearer tokens, and Entra ID
- **Process responses consistently** using standardized `ApiResponse<T>` models
- **Manage multiple API endpoints** with named client configurations
- **Integrate with ASP.NET Core** applications using extension methods

## Quick Start

### 1. Install the Package

```bash
dotnet add package MX.Api.Client
```

### 2. Basic Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add API client services
builder.Services.AddApiClient()
    .WithBaseUrl("https://api.example.com")
    .WithApiKeyAuthentication("your-api-key");

// Register your API client
builder.Services.AddTransient<UserApiClient>();

var app = builder.Build();
```

### 3. Create a Simple Client

```csharp
public class UserApiClient : BaseApi
{
    private readonly ILogger<UserApiClient> _logger;

    public UserApiClient(
        ILogger<UserApiClient> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptions<ApiClientOptions> options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
        _logger = logger;
    }

    public async Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving user {UserId}", userId);
            
            var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            
            return response.ToApiResponse<User>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to retrieve user {UserId}", userId);
            var errorResponse = new ApiResponse<User>(new ApiError("CLIENT_ERROR", "Failed to retrieve user"));
            return new ApiResult<User>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
```

### 4. Use the Client

```csharp
public class UserService
{
    private readonly UserApiClient _userApiClient;

    public UserService(UserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        var result = await _userApiClient.GetUserAsync(userId);
        
        if (result.IsSuccess && result.Result?.Data != null)
        {
            return result.Result.Data;
        }
        
        // Handle error cases
        if (result.IsNotFound)
        {
            return null; // User not found
        }
        
        throw new ApplicationException($"Failed to get user: {result.StatusCode}");
    }
}
```

## Authentication Methods

### 1. API Key Authentication

```csharp
// Simple API key in header
builder.Services.AddApiClient()
    .WithBaseUrl("https://api.example.com")
    .WithApiKeyAuthentication("your-api-key");

// Custom header name
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://api.example.com")
    .WithApiKey("your-api-key", "X-Custom-Api-Key"));
```

### 2. Bearer Token Authentication

```csharp
// Static bearer token
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://api.example.com")
    .WithBearerToken("your-bearer-token"));
```

### 3. Entra ID (Azure AD) Authentication

```csharp
// Using DefaultAzureCredential
builder.Services.AddApiClient()
    .WithBaseUrl("https://api.example.com")
    .WithEntraIdAuthentication("api://your-api-audience");

// With specific tenant
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://api.example.com")
    .WithEntraIdAuthentication("api://your-api-audience", "your-tenant-id"));
```

### 4. Client Credentials Flow

```csharp
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://api.example.com")
    .WithClientCredentials(
        audience: "api://your-api-audience",
        tenantId: "your-tenant-id",
        clientId: "your-client-id",
        clientSecret: "your-client-secret"));
```

### 5. Multiple Authentication Methods

```csharp
// Common scenario: Azure API Management + Backend API
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://your-api.azure-api.net")
    .WithSubscriptionKey("your-apim-subscription-key")     // For APIM
    .WithEntraIdAuthentication("api://backend-api"));      // For backend
```

### 6. Custom Credential Provider

```csharp
// Register custom provider
builder.Services.AddApiClientWithCustomCredentialProvider<CustomTokenProvider>();

public class CustomTokenProvider : ITokenCredentialProvider
{
    public async Task<TokenCredential> GetTokenCredentialAsync(CancellationToken cancellationToken = default)
    {
        // Your custom authentication logic
        return new ClientSecretCredential("tenant", "client", "secret");
    }
}
```

## Building API Clients

### Registration Patterns Overview

The MX.Api.Client library provides two registration patterns depending on your complexity needs:

#### Simple Registration: AddApiClient<TInterface, TImplementation>

Best for straightforward scenarios with basic configuration needs:

```csharp
// Register your client with interface
builder.Services.AddApiClient<IProductApiClient, ProductApiClient>(options =>
{
    options.WithBaseUrl("https://products.example.com")
           .WithApiKeyAuthentication("your-api-key");
});

// Define the interface for better testability
public interface IProductApiClient
{
    Task<ApiResult<CollectionModel<Product>>> GetProductsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ApiResult<Product>> GetProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<ApiResult<Product>> CreateProductAsync(CreateProductRequest product, CancellationToken cancellationToken = default);
}

// Implement using standard BaseApi
public class ProductApiClient : BaseApi, IProductApiClient
{
    public ProductApiClient(
        ILogger<BaseApi<ApiClientOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        ApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    // Implement your API methods...
}
```

#### Advanced Registration: AddTypedApiClient<TInterface, TImplementation, TOptions, TBuilder>

Best for complex scenarios requiring custom configuration options:

```csharp
// Define custom options
public class ProductApiClientOptions : ApiClientOptionsBase
{
    public string ApiVersion { get; set; } = "v1";
    public int DefaultPageSize { get; set; } = 20;
    public bool EnableCaching { get; set; } = true;
    
    public override void Validate()
    {
        base.Validate();
        if (DefaultPageSize <= 0)
            throw new InvalidOperationException("DefaultPageSize must be greater than 0");
    }
}

// Define custom builder
public class ProductApiClientOptionsBuilder : ApiClientOptionsBuilder<ProductApiClientOptions, ProductApiClientOptionsBuilder>
{
    public ProductApiClientOptionsBuilder WithApiVersion(string version)
    {
        Options.ApiVersion = version;
        return this;
    }
    
    public ProductApiClientOptionsBuilder WithDefaultPageSize(int pageSize)
    {
        Options.DefaultPageSize = pageSize;
        return this;
    }
    
    public ProductApiClientOptionsBuilder EnableCaching(bool enable = true)
    {
        Options.EnableCaching = enable;
        return this;
    }
}

// Register with strongly-typed options
builder.Services.AddTypedApiClient<IProductApiClient, ProductApiClient, ProductApiClientOptions, ProductApiClientOptionsBuilder>(options =>
{
    options.WithBaseUrl("https://products.example.com")
           .WithApiKeyAuthentication("your-api-key")
           .WithApiVersion("v2")
           .WithDefaultPageSize(50)
           .EnableCaching(true);
});

// Implement using strongly-typed BaseApi
public class ProductApiClient : BaseApi<ProductApiClientOptions>, IProductApiClient
{
    public ProductApiClient(
        ILogger<BaseApi<ProductApiClientOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        ProductApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<CollectionModel<Product>>> GetProductsAsync(
        int page = 1, 
        int pageSize = 0, // 0 = use default
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("products", Method.Get, cancellationToken);
        
        // Use custom options
        request.AddQueryParameter("version", Options.ApiVersion);
        request.AddQueryParameter("page", page.ToString());
        request.AddQueryParameter("pageSize", (pageSize > 0 ? pageSize : Options.DefaultPageSize).ToString());
        
        if (Options.EnableCaching)
        {
            request.AddHeader("Cache-Control", "max-age=300");
        }

        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<CollectionModel<Product>>();
    }

    // Other API methods...
}
```

### When to Use Each Pattern

**Use `AddApiClient<TInterface, TImplementation>`** when:
- You have simple configuration requirements
- Standard `ApiClientOptions` are sufficient  
- You want minimal setup and registration code
- You're building a single-purpose API client

**Use `AddTypedApiClient<TInterface, TImplementation, TOptions, TBuilder>`** when:
- You need custom configuration properties beyond the standard options
- You want compile-time validation of configuration
- You're building a reusable client library for distribution
- You need advanced configuration scenarios with multiple related settings
- You want to enforce specific configuration patterns across teams

### Basic Client Pattern

```csharp
public class ProductApiClient : BaseApi
{
    private readonly ILogger<ProductApiClient> _logger;

    public ProductApiClient(
        ILogger<ProductApiClient> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptions<ApiClientOptions> options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
        _logger = logger;
    }

    public async Task<ApiResult<CollectionModel<Product>>> GetProductsAsync(
        int page = 1,
        int pageSize = 10,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("products", Method.Get, cancellationToken);
            
            // Add query parameters
            request.AddQueryParameter("page", page.ToString());
            request.AddQueryParameter("pageSize", pageSize.ToString());
            if (!string.IsNullOrEmpty(category))
                request.AddQueryParameter("category", category);

            var response = await ExecuteAsync(request, false, cancellationToken);
            return response.ToApiResponse<CollectionModel<Product>>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to get products");
            return CreateErrorResult<CollectionModel<Product>>("Failed to retrieve products");
        }
    }

    public async Task<ApiResult<Product>> CreateProductAsync(
        CreateProductRequest product,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("products", Method.Post, cancellationToken);
            request.AddJsonBody(product);

            var response = await ExecuteAsync(request, false, cancellationToken);
            return response.ToApiResponse<Product>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create product");
            return CreateErrorResult<Product>("Failed to create product");
        }
    }

    public async Task<ApiResult<Product>> UpdateProductAsync(
        int id,
        UpdateProductRequest product,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync($"products/{id}", Method.Put, cancellationToken);
            request.AddJsonBody(product);

            var response = await ExecuteAsync(request, false, cancellationToken);
            return response.ToApiResponse<Product>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update product {ProductId}", id);
            return CreateErrorResult<Product>($"Failed to update product {id}");
        }
    }

    public async Task<ApiResult> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync($"products/{id}", Method.Delete, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            
            return response.ToApiResponse();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}", id);
            return CreateErrorResult($"Failed to delete product {id}");
        }
    }

    private ApiResult<T> CreateErrorResult<T>(string message)
    {
        var errorResponse = new ApiResponse<T>(new ApiError("CLIENT_ERROR", message));
        return new ApiResult<T>(HttpStatusCode.InternalServerError, errorResponse);
    }

    private ApiResult CreateErrorResult(string message)
    {
        var errorResponse = new ApiResponse(new ApiError("CLIENT_ERROR", message));
        return new ApiResult(HttpStatusCode.InternalServerError, errorResponse);
    }
}
```

### Handling Different Response Types

```csharp
public class WeatherApiClient : BaseApi
{
    public WeatherApiClient(/* dependencies */) : base(/* base parameters */) { }

    // Handle single object responses
    public async Task<ApiResult<WeatherForecast>> GetCurrentWeatherAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"weather/current", Method.Get, cancellationToken);
        request.AddQueryParameter("location", location);
        
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<WeatherForecast>();
    }

    // Handle collection responses
    public async Task<ApiResult<IEnumerable<WeatherForecast>>> GetForecastAsync(
        string location,
        int days,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"weather/forecast", Method.Get, cancellationToken);
        request.AddQueryParameter("location", location);
        request.AddQueryParameter("days", days.ToString());
        
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<IEnumerable<WeatherForecast>>();
    }

    // Handle paginated responses (if API supports MX pattern)
    public async Task<ApiResult<CollectionModel<WeatherAlert>>> GetAlertsAsync(
        string region,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"weather/alerts", Method.Get, cancellationToken);
        request.AddQueryParameter("region", region);
        request.AddQueryParameter("page", page.ToString());
        request.AddQueryParameter("pageSize", pageSize.ToString());
        
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<CollectionModel<WeatherAlert>>();
    }

    // Handle operations with no response body
    public async Task<ApiResult> ResetCacheAsync(CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("weather/cache/reset", Method.Post, cancellationToken);
        var response = await ExecuteAsync(request, false, cancellationToken);
        
        return response.ToApiResponse();
    }
}
```

## Multiple API Clients

### Named Client Configurations

```csharp
// Program.cs - Register multiple API configurations
var builder = WebApplication.CreateBuilder(args);

// Register the base API client service
builder.Services.AddApiClient();

// Configure multiple named clients
builder.Services.Configure<ApiClientOptions>("UsersApi", options => options
    .WithBaseUrl("https://users.api.example.com")
    .WithApiKeyAuthentication("users-api-key"));

builder.Services.Configure<ApiClientOptions>("OrdersApi", options => options
    .WithBaseUrl("https://orders.api.example.com")
    .WithEntraIdAuthentication("api://orders-api"));

builder.Services.Configure<ApiClientOptions>("NotificationApi", options => options
    .WithBaseUrl("https://notifications.api.example.com")
    .WithClientCredentials("api://notifications", "tenant", "client", "secret"));

// Register typed clients
builder.Services.AddTransient<UsersApiClient>();
builder.Services.AddTransient<OrdersApiClient>();
builder.Services.AddTransient<NotificationApiClient>();
```

### Named Client Implementation

```csharp
public class UsersApiClient : BaseApi
{
    public UsersApiClient(
        ILogger<UsersApiClient> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptionsSnapshot<ApiClientOptions> optionsSnapshot)
        : base(logger, apiTokenProvider, restClientService, optionsSnapshot, "UsersApi") // Named configuration
    {
    }

    public async Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<User>();
    }
}

public class OrdersApiClient : BaseApi
{
    public OrdersApiClient(
        ILogger<OrdersApiClient> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptionsSnapshot<ApiClientOptions> optionsSnapshot)
        : base(logger, apiTokenProvider, restClientService, optionsSnapshot, "OrdersApi") // Named configuration
    {
    }

    public async Task<ApiResult<Order>> GetOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"orders/{orderId}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<Order>();
    }
}
```

### Service Registration Extensions

```csharp
// Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersApiClient(
        this IServiceCollection services,
        string baseUrl,
        string apiKey)
    {
        services.AddApiClient();
        services.Configure<ApiClientOptions>("UsersApi", options => options
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiKey));
        services.AddTransient<UsersApiClient>();
        
        return services;
    }

    public static IServiceCollection AddOrdersApiClient(
        this IServiceCollection services,
        string baseUrl,
        string audience)
    {
        services.AddApiClient();
        services.Configure<ApiClientOptions>("OrdersApi", options => options
            .WithBaseUrl(baseUrl)
            .WithEntraIdAuthentication(audience));
        services.AddTransient<OrdersApiClient>();
        
        return services;
    }
}

// Usage in Program.cs
builder.Services.AddUsersApiClient("https://users.api.example.com", "users-api-key");
builder.Services.AddOrdersApiClient("https://orders.api.example.com", "api://orders-api");
```

## Error Handling

### Response Status Checking

```csharp
public async Task<User?> GetUserSafelyAsync(string userId)
{
    var result = await _userApiClient.GetUserAsync(userId);

    // Check different response scenarios
    if (result.IsSuccess)
    {
        return result.Result?.Data;
    }

    if (result.IsNotFound)
    {
        _logger.LogWarning("User {UserId} not found", userId);
        return null;
    }

    if (result.IsUnauthorized)
    {
        _logger.LogError("Unauthorized access when getting user {UserId}", userId);
        throw new UnauthorizedAccessException("API access denied");
    }

    if (result.IsBadRequest)
    {
        _logger.LogError("Bad request when getting user {UserId}: {StatusCode}", userId, result.StatusCode);
        throw new ArgumentException($"Invalid user ID: {userId}");
    }

    // Handle other error cases
    _logger.LogError("Failed to get user {UserId}: {StatusCode}", userId, result.StatusCode);
    throw new ApplicationException($"API call failed: {result.StatusCode}");
}
```

### Detailed Error Information

```csharp
public async Task<ApiResult<Product>> CreateProductWithValidationAsync(CreateProductRequest product)
{
    var result = await _productApiClient.CreateProductAsync(product);

    if (!result.IsSuccess && result.Result?.Errors != null)
    {
        foreach (var error in result.Result.Errors)
        {
            _logger.LogWarning("API Error - Code: {Code}, Message: {Message}, Detail: {Detail}",
                error.Code, error.Message, error.Detail);
        }
    }

    return result;
}
```

### Retry Logic for Specific Scenarios

```csharp
public class ResilientApiClient : BaseApi
{
    public async Task<ApiResult<T>> GetWithRetryAsync<T>(
        string endpoint,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var request = await CreateRequestAsync(endpoint, Method.Get, cancellationToken);
                var response = await ExecuteAsync(request, false, cancellationToken);
                var result = response.ToApiResponse<T>();

                // Success or non-retryable error
                if (result.IsSuccess || !IsRetryableError(result.StatusCode))
                {
                    return result;
                }

                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException && attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay, cancellationToken);
            }
        }

        // All retries failed
        var errorResponse = new ApiResponse<T>(new ApiError("RETRY_EXHAUSTED", "All retry attempts failed"));
        return new ApiResult<T>(HttpStatusCode.InternalServerError, errorResponse);
    }

    private static bool IsRetryableError(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.InternalServerError ||
               statusCode == HttpStatusCode.BadGateway ||
               statusCode == HttpStatusCode.ServiceUnavailable ||
               statusCode == HttpStatusCode.GatewayTimeout ||
               statusCode == HttpStatusCode.TooManyRequests;
    }
}
```

## Advanced Scenarios

### Custom Headers and Request Modification

```csharp
public class CustomApiClient : BaseApi
{
    public async Task<ApiResult<CustomResponse>> CustomRequestAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("custom-endpoint", Method.Get, cancellationToken);
        
        // Add custom headers
        request.AddHeader("X-Correlation-ID", correlationId);
        request.AddHeader("X-Client-Version", "1.0.0");
        
        // Modify request before execution
        request.Timeout = TimeSpan.FromMinutes(5); // Custom timeout
        
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<CustomResponse>();
    }
}
```

### File Upload/Download

```csharp
public class FileApiClient : BaseApi
{
    public async Task<ApiResult<FileUploadResponse>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("files/upload", Method.Post, cancellationToken);
            request.AddFile("file", fileStream.ToArray(), fileName, contentType);
            
            var response = await ExecuteAsync(request, false, cancellationToken);
            return response.ToApiResponse<FileUploadResponse>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            var errorResponse = new ApiResponse<FileUploadResponse>(
                new ApiError("UPLOAD_ERROR", "Failed to upload file"));
            return new ApiResult<FileUploadResponse>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<Stream> DownloadFileAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"files/{fileId}/download", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, false, cancellationToken);
        
        if (response.IsSuccessful && response.RawBytes != null)
        {
            return new MemoryStream(response.RawBytes);
        }
        
        throw new ApplicationException($"Failed to download file: {response.StatusCode}");
    }
}
```

### Caching Responses

```csharp
public class CachedApiClient : BaseApi
{
    private readonly IMemoryCache _cache;

    public CachedApiClient(
        IMemoryCache cache,
        /* other dependencies */)
        : base(/* base parameters */)
    {
        _cache = cache;
    }

    public async Task<ApiResult<Product>> GetProductWithCacheAsync(
        int productId,
        TimeSpan? cacheExpiry = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{productId}";
        
        if (_cache.TryGetValue(cacheKey, out ApiResult<Product> cachedResult))
        {
            _logger.LogDebug("Returning cached product {ProductId}", productId);
            return cachedResult;
        }

        var result = await GetProductAsync(productId, cancellationToken);
        
        if (result.IsSuccess)
        {
            var expiry = cacheExpiry ?? TimeSpan.FromMinutes(5);
            _cache.Set(cacheKey, result, expiry);
            _logger.LogDebug("Cached product {ProductId} for {Expiry}", productId, expiry);
        }

        return result;
    }
}
```

## Best Practices

### 1. Configuration Management

```csharp
// appsettings.json
{
  "ApiClients": {
    "UsersApi": {
      "BaseUrl": "https://users.api.example.com",
      "ApiKey": "your-users-api-key",
      "MaxRetryCount": 3,
      "TimeoutSeconds": 30
    },
    "OrdersApi": {
      "BaseUrl": "https://orders.api.example.com",
      "Audience": "api://orders-api",
      "TenantId": "your-tenant-id",
      "MaxRetryCount": 5,
      "TimeoutSeconds": 60
    }
  }
}

// Configuration binding
public class ApiClientSettings
{
    public Dictionary<string, ApiClientConfig> ApiClients { get; set; } = new();
}

public class ApiClientConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? Audience { get; set; }
    public string? TenantId { get; set; }
    public int MaxRetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}

// Registration
var apiSettings = builder.Configuration.GetSection("ApiClients").Get<ApiClientSettings>();

foreach (var (name, config) in apiSettings.ApiClients)
{
    builder.Services.Configure<ApiClientOptions>(name, options =>
    {
        options.WithBaseUrl(config.BaseUrl)
               .WithMaxRetryCount(config.MaxRetryCount);
               
        if (!string.IsNullOrEmpty(config.ApiKey))
            options.WithApiKeyAuthentication(config.ApiKey);
            
        if (!string.IsNullOrEmpty(config.Audience))
            options.WithEntraIdAuthentication(config.Audience, config.TenantId);
    });
}
```

### 2. Health Checks

```csharp
public class ApiClientHealthCheck : IHealthCheck
{
    private readonly UserApiClient _userApiClient;

    public ApiClientHealthCheck(UserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Make a lightweight health check call
            var result = await _userApiClient.GetHealthAsync(cancellationToken);
            
            return result.IsSuccess 
                ? HealthCheckResult.Healthy("Users API is responding")
                : HealthCheckResult.Unhealthy($"Users API returned {result.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Users API is not reachable", ex);
        }
    }
}

// Registration
builder.Services.AddHealthChecks()
    .AddCheck<ApiClientHealthCheck>("users-api");
```

### 3. Logging and Monitoring

```csharp
public class MonitoredApiClient : BaseApi
{
    private readonly IMetrics _metrics;

    public async Task<ApiResult<T>> MonitoredRequestAsync<T>(
        string endpoint,
        Method method,
        CancellationToken cancellationToken = default)
    {
        using var timer = _metrics.Measure.Timer.Time("api_request_duration", new MetricTags("endpoint", endpoint));
        
        try
        {
            var request = await CreateRequestAsync(endpoint, method, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            var result = response.ToApiResponse<T>();

            // Record metrics
            _metrics.Measure.Counter.Increment("api_requests_total", 
                new MetricTags("endpoint", endpoint, "status", result.StatusCode.ToString()));

            if (!result.IsSuccess)
            {
                _metrics.Measure.Counter.Increment("api_requests_failed",
                    new MetricTags("endpoint", endpoint, "status", result.StatusCode.ToString()));
            }

            return result;
        }
        catch (Exception ex)
        {
            _metrics.Measure.Counter.Increment("api_requests_error",
                new MetricTags("endpoint", endpoint, "error", ex.GetType().Name));
            throw;
        }
    }
}
```

### 4. Testing

```csharp
// Unit test with mocked dependencies
[Test]
public async Task GetUser_WhenUserExists_ReturnsUser()
{
    // Arrange
    var mockTokenProvider = new Mock<IApiTokenProvider>();
    var mockRestClient = new Mock<IRestClientService>();
    var mockOptions = Options.Create(new ApiClientOptions { BaseUrl = "https://test.api" });
    
    var expectedUser = new User { Id = "123", Name = "Test User" };
    var mockResponse = new RestResponse
    {
        IsSuccessful = true,
        StatusCode = HttpStatusCode.OK,
        Content = JsonConvert.SerializeObject(new ApiResponse<User>(expectedUser))
    };
    
    mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(mockResponse);

    var client = new UserApiClient(Mock.Of<ILogger<UserApiClient>>(), 
        mockTokenProvider.Object, mockRestClient.Object, mockOptions);

    // Act
    var result = await client.GetUserAsync("123");

    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual("123", result.Result?.Data?.Id);
}

// Integration test with TestServer
[Test]
public async Task GetUser_Integration_ReturnsExpectedUser()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/users/123");
    var content = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<User>>(content);

    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    Assert.IsNotNull(apiResponse?.Data);
    Assert.AreEqual("123", apiResponse.Data.Id);
}
```

### 5. Performance Optimization

```csharp
// Implement connection pooling and reuse
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://api.example.com")
    .WithMaxRetryCount(3));

// Use HTTP client factory for efficient connection management
builder.Services.AddHttpClient();

// For high-throughput scenarios, consider batching
public class BatchApiClient : BaseApi
{
    public async Task<IEnumerable<ApiResult<User>>> GetUsersInBatchAsync(
        IEnumerable<string> userIds,
        int batchSize = 10,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ApiResult<User>>();
        var batches = userIds.Chunk(batchSize);

        foreach (var batch in batches)
        {
            var tasks = batch.Select(id => GetUserAsync(id, cancellationToken));
            var batchResults = await Task.WhenAll(tasks);
            results.AddRange(batchResults);
        }

        return results;
    }
}
```

This comprehensive guide should help you effectively consume APIs using the MX API Abstractions client library, whether you're working with APIs that follow the MX pattern or traditional REST APIs.

## Related Documentation

For more advanced patterns and specific scenarios, see:

- **[Versioned API Client Pattern](implementing-versioned-api-client.md)** - Implement clients that support multiple API versions
- **[API Provider Implementation](implementing-api-provider.md)** - Guide for teams building APIs
- **[API Design Patterns](api-design-v2.md)** - Understanding the underlying design principles
