# MX.Api.Client

Resilient REST API client library for building robust .NET applications that consume APIs. Provides authentication, retry policies, token management, and standardized response processing built on top of MX.Api.Abstractions.

## Installation

```bash
dotnet add package MX.Api.Client
```

## Key Features

- **🔐 Multi-Authentication Support** - API keys, Bearer tokens, and Entra ID with automatic token management  
- **🛡️ Built-in Resilience** - Retry policies, circuit breakers, and exponential backoff
- **⚡ High Performance** - Thread-safe operations with efficient caching and connection pooling
- **🔄 Standardized Responses** - Uses `ApiResponse<T>` models for consistent error handling
- **📊 Comprehensive Logging** - Integration with Microsoft.Extensions.Logging for diagnostics

## Quick Start

### 1. Basic Setup

```csharp
// Program.cs
builder.Services.AddApiClient()
    .WithBaseUrl("https://api.example.com")
    .WithApiKeyAuthentication("your-api-key");

// Register your client
builder.Services.AddTransient<MyApiClient>();
```

### 2. Create Your Client

```csharp
public class MyApiClient : BaseApi
{
    private readonly ILogger<MyApiClient> _logger;

    public MyApiClient(
        ILogger<MyApiClient> logger,
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
            var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            return response.ToApiResponse<User>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", userId);
            var errorResponse = new ApiResponse<User>(new ApiError("CLIENT_ERROR", "Failed to retrieve user"));
            return new ApiResult<User>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
```

### 3. Use Your Client

```csharp
public class UserService
{
    private readonly MyApiClient _apiClient;

    public UserService(MyApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        var result = await _apiClient.GetUserAsync(userId);
        
        if (result.IsSuccess)
            return result.Result?.Data;
            
        if (result.IsNotFound)
            return null;
            
        throw new ApplicationException($"API error: {result.StatusCode}");
    }
}
```

## Registration Patterns

### Simple Registration with AddApiClient

The `AddApiClient<TInterface, TImplementation>` method provides a simplified registration pattern for basic scenarios:

```csharp
// Simple registration with interface and implementation
builder.Services.AddApiClient<IUsersApiClient, UsersApiClient>(options =>
{
    options.WithBaseUrl("https://users.example.com")
           .WithApiKeyAuthentication("your-api-key");
});

// The client interface
public interface IUsersApiClient
{
    Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<ApiResult<CollectionModel<User>>> GetUsersAsync(FilterOptions? filter = null, CancellationToken cancellationToken = default);
}

// The client implementation
public class UsersApiClient : BaseApi, IUsersApiClient
{
    public UsersApiClient(
        ILogger<BaseApi<ApiClientOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        ApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<User>();
    }

    // Other API methods...
}
```

### Advanced Registration with AddTypedApiClient

The `AddTypedApiClient<TInterface, TImplementation, TOptions, TBuilder>` method provides full control over options types for complex scenarios:

```csharp
// Define custom options class
public class UsersApiClientOptions : ApiClientOptionsBase
{
    public string? ApiVersion { get; set; }
    public int CacheTimeoutMinutes { get; set; } = 5;
    
    // Custom validation
    public override void Validate()
    {
        base.Validate();
        if (string.IsNullOrEmpty(ApiVersion))
            throw new InvalidOperationException("ApiVersion is required for UsersApiClient");
    }
}

// Define custom builder
public class UsersApiClientOptionsBuilder : ApiClientOptionsBuilder<UsersApiClientOptions, UsersApiClientOptionsBuilder>
{
    public UsersApiClientOptionsBuilder WithApiVersion(string version)
    {
        Options.ApiVersion = version;
        return this;
    }
    
    public UsersApiClientOptionsBuilder WithCacheTimeout(int minutes)
    {
        Options.CacheTimeoutMinutes = minutes;
        return this;
    }
}

// Register with strongly-typed options
builder.Services.AddTypedApiClient<IUsersApiClient, UsersApiClient, UsersApiClientOptions, UsersApiClientOptionsBuilder>(options =>
{
    options.WithBaseUrl("https://users.example.com")
           .WithApiKeyAuthentication("your-api-key")
           .WithApiVersion("v2")
           .WithCacheTimeout(10);
});

// The client implementation using strongly-typed options
public class UsersApiClient : BaseApi<UsersApiClientOptions>, IUsersApiClient
{
    public UsersApiClient(
        ILogger<BaseApi<UsersApiClientOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        UsersApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
        
        // Access strongly-typed options
        if (!string.IsNullOrEmpty(Options.ApiVersion))
        {
            request.AddQueryParameter("version", Options.ApiVersion);
        }
        
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<User>();
    }

    // Other API methods...
}
```

### When to Use Each Pattern

**Use `AddApiClient<TInterface, TImplementation>`** when:
- You have simple configuration needs
- Default `ApiClientOptions` are sufficient
- You want minimal setup overhead
- You're building straightforward API clients

**Use `AddTypedApiClient<TInterface, TImplementation, TOptions, TBuilder>`** when:
- You need custom configuration properties
- You want strongly-typed options validation
- You're building reusable API client libraries
- You need complex configuration scenarios
- You want to enforce specific configuration patterns

## Authentication Methods

### API Key Authentication

```csharp
// Default header (X-API-Key)
builder.Services.AddApiClient()
    .WithApiKeyAuthentication("your-api-key");

// Custom header
builder.Services.Configure<ApiClientOptions>(options => options
    .WithApiKey("your-api-key", "X-Custom-Api-Key"));
```

### Bearer Token Authentication

```csharp
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBearerToken("your-bearer-token"));
```

### Entra ID (Azure AD) Authentication

```csharp
// Using DefaultAzureCredential
builder.Services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience");

// With specific tenant
builder.Services.Configure<ApiClientOptions>(options => options
    .WithEntraIdAuthentication("api://your-api-audience", "your-tenant-id"));
```

### Client Credentials Flow

```csharp
builder.Services.Configure<ApiClientOptions>(options => options
    .WithClientCredentials(
        audience: "api://your-api",
        tenantId: "tenant-id",
        clientId: "client-id", 
        clientSecret: "client-secret"));
```

### Multiple Authentication Methods

```csharp
// Azure API Management + Backend API
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://your-api.azure-api.net")
    .WithSubscriptionKey("apim-subscription-key")     // For APIM
    .WithEntraIdAuthentication("api://backend-api")); // For backend
```

## Multiple API Clients

### Named Configurations

```csharp
// Register base service
builder.Services.AddApiClient();

// Configure multiple named clients
builder.Services.Configure<ApiClientOptions>("UsersApi", options => options
    .WithBaseUrl("https://users.example.com")
    .WithApiKeyAuthentication("users-api-key"));

builder.Services.Configure<ApiClientOptions>("OrdersApi", options => options
    .WithBaseUrl("https://orders.example.com")
    .WithEntraIdAuthentication("api://orders-api"));

// Register typed clients
builder.Services.AddTransient<UsersApiClient>();
builder.Services.AddTransient<OrdersApiClient>();
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
        : base(logger, apiTokenProvider, restClientService, optionsSnapshot, "UsersApi")
    {
    }

    // API methods...
}
```

## Advanced Features

### Custom Request Configuration

```csharp
public async Task<ApiResult<T>> CustomRequestAsync<T>(CancellationToken cancellationToken = default)
{
    var request = await CreateRequestAsync("endpoint", Method.Get, cancellationToken);
    
    // Add custom headers
    request.AddHeader("X-Correlation-ID", Guid.NewGuid().ToString());
    request.AddHeader("X-Client-Version", "1.0.0");
    
    // Custom timeout
    request.Timeout = TimeSpan.FromMinutes(5);
    
    var response = await ExecuteAsync(request, false, cancellationToken);
    return response.ToApiResponse<T>();
}
```

### File Upload/Download

```csharp
// File upload
public async Task<ApiResult<UploadResponse>> UploadFileAsync(
    Stream fileStream, 
    string fileName, 
    CancellationToken cancellationToken = default)
{
    var request = await CreateRequestAsync("files/upload", Method.Post, cancellationToken);
    request.AddFile("file", fileStream.ToArray(), fileName, "application/octet-stream");
    
    var response = await ExecuteAsync(request, false, cancellationToken);
    return response.ToApiResponse<UploadResponse>();
}

// File download  
public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
{
    var request = await CreateRequestAsync($"files/{fileId}", Method.Get, cancellationToken);
    var response = await ExecuteAsync(request, false, cancellationToken);
    
    if (response.IsSuccessful && response.RawBytes != null)
        return new MemoryStream(response.RawBytes);
        
    throw new ApplicationException($"Download failed: {response.StatusCode}");
}
```

### Error Handling

```csharp
public async Task<User?> GetUserSafelyAsync(string userId)
{
    var result = await GetUserAsync(userId);

    if (result.IsSuccess)
        return result.Result?.Data;

    if (result.IsNotFound)
        return null;

    if (result.IsUnauthorized)
        throw new UnauthorizedAccessException("API access denied");

    if (result.IsBadRequest)
        throw new ArgumentException($"Invalid user ID: {userId}");

    throw new ApplicationException($"API call failed: {result.StatusCode}");
}
```

## Configuration

### appsettings.json

```json
{
  "ApiClient": {
    "BaseUrl": "https://api.example.com",
    "MaxRetryCount": 3,
    "TimeoutSeconds": 30,
    "ApiKey": "your-api-key"
  }
}
```

### Programmatic Configuration

```csharp
builder.Services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://api.example.com")
    .WithMaxRetryCount(5)
    .WithApiKeyAuthentication("your-api-key"));
```

## Dependencies

This package depends on:
- **MX.Api.Abstractions** - Core response models and interfaces
- **Azure.Identity** - For Entra ID authentication
- **RestSharp** - HTTP client functionality
- **Polly** - Resilience patterns and retry policies
- **Microsoft.Extensions.*** - Logging, configuration, and dependency injection

## Documentation

- **[📖 Implementation Guide - API Consumers](../../docs/implementing-api-consumer.md)** - Complete guide for consuming APIs
- **[📖 API Design Patterns](../../docs/api-design-v2.md)** - Understanding the underlying patterns
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
            return new ApiResult<ResourceDto>(HttpStatusCode.InternalServerError, errorResponse);
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

// Or configure via options
services.Configure<ApiClientOptions>(options => options
    .WithApiKeyAuthentication("your-api-key", "X-API-Key"));

// For Azure API Management subscription keys
services.Configure<ApiClientOptions>(options => options
    .WithSubscriptionKey("your-subscription-key")); // Uses Ocp-Apim-Subscription-Key header
```

### Entra ID Authentication

Use this when your API requires OAuth tokens from Entra ID (formerly Azure AD).

```csharp
services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience");

// Or configure via options
services.Configure<ApiClientOptions>(options => options
    .WithEntraIdAuthentication("api://your-api-audience"));
```

### Multiple Authentication Methods

For APIs behind Azure API Management that require both subscription keys and identity tokens:

```csharp
services.Configure<ApiClientOptions>(options => options
    .WithBaseUrl("https://your-api-via-apim.azure-api.net")
    .WithSubscriptionKey("your-apim-subscription-key")      // For API Management
    .WithEntraIdAuthentication("api://your-api-audience")); // For underlying API

// This will result in requests having both:
// - Ocp-Apim-Subscription-Key: your-apim-subscription-key
// - Authorization: Bearer <entra-id-token>
```

Custom combinations:

```csharp
services.Configure<ApiClientOptions>(options => options
    .WithApiKeyAuthentication("primary-key", "X-Primary-Key")
    .WithApiKeyAuthentication("secondary-key", "X-Secondary-Key")
    .WithEntraIdAuthentication("api://your-api-audience"));
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
        return new ApiResult<CollectionModel<ResourceDto>>(HttpStatusCode.InternalServerError, errorResponse);
    }
}
```
