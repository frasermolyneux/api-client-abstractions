# MX.Api.Client

A comprehensive implementation for building resilient, authenticated REST API clients as part of the MX API Abstractions approach. This package provides base classes, interfaces, and utilities for creating API clients with features such as authentication, token management, request execution, and response processing. Built on top of MX.Api.Abstractions, it ensures consistent API interaction patterns across your applications.

## Features

- Token-based authentication support with automatic acquisition and caching
- API key authentication with resilient handling
- Entra ID (Azure AD) authentication with DefaultAzureCredential support
- Configurable retry policies with exponential backoff
- Thread-safe REST client management
- Standardized response handling with ApiResponse<T> model
- Support for filtering, pagination, and OData-like queries
- Comprehensive error handling and validation
- Extension methods for common operations

## Installation

```bash
dotnet add package MX.Api.Client
```

## Core Components

### BaseApi

The `BaseApi` class provides the foundation for building API clients:

```csharp
public abstract class BaseApi
{
    // Constructor
    protected BaseApi(
        ILogger logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptions<ApiClientOptions> options);
        
    // Create authenticated requests
    protected Task<RestRequest> CreateRequestAsync(
        string resourcePath,
        Method method,
        CancellationToken cancellationToken = default);
        
    // Execute requests
    protected Task<RestResponse> ExecuteAsync(
        RestRequest request,
        bool rethrowExceptions = true,
        CancellationToken cancellationToken = default);
        
    // Execute requests with strong typing
    protected Task<ApiResult<T>> ExecuteWithResponseWrapperAsync<T>(
        RestRequest request,
        bool rethrowExceptions = true,
        CancellationToken cancellationToken = default);
        
    protected Task<ApiResponse<T>> ExecuteWithApiResponseAsync<T>(
        RestRequest request,
        bool rethrowExceptions = true,
        CancellationToken cancellationToken = default);
        
    protected Task<T> ExecuteWithResultAsync<T>(
        RestRequest request,
        bool rethrowExceptions = true,
        CancellationToken cancellationToken = default);
}
```

### IApiTokenProvider

Interface for token-based authentication providers:

```csharp
public interface IApiTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
```

### IRestClientService

Interface for REST client operations:

```csharp
public interface IRestClientService
{
    Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default);
    void ConfigureOptions(Action<RestClientOptions> configureOptions);
}
```

### ApiClientOptions

Configuration options for API clients:

```csharp
public class ApiClientOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public AuthenticationOptions? AuthenticationOptions { get; set; }
    public int MaxRetryCount { get; set; } = 3;
    
    // Fluent configuration methods
    public ApiClientOptions WithApiKeyAuthentication(string apiKey, string headerName = "Ocp-Apim-Subscription-Key")
    public ApiClientOptions WithAuthentication(AuthenticationOptions authenticationOptions)
}

public class ApiKeyAuthenticationOptions : AuthenticationOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string HeaderName { get; set; } = "Ocp-Apim-Subscription-Key";
    public override AuthenticationType AuthenticationType => AuthenticationType.ApiKey;
}
```

## Usage Examples

### Setup and Registration

```csharp
// Add API client to services
public void ConfigureServices(IServiceCollection services)
{
    // Basic registration with API key authentication
    services.AddApiClient()
            .WithBaseUrl("https://api.example.com")
            .WithApiKeyAuthentication("your-api-key", "X-Custom-Api-Key");
        
    // Alternative configuration using configuration action
    services.AddApiClient()
            .WithOptions(options =>
            {
                options.BaseUrl = "https://api.example.com";
                options.MaxRetryCount = 3;
                options.AuthenticationOptions = new ApiKeyAuthenticationOptions
                {
                    ApiKey = "your-api-key",
                    HeaderName = "X-Custom-Api-Key"
                };
            });
}
```

### Creating a Custom API Client

```csharp
public class UsersApiClient : BaseApi
{
    private readonly ILogger<UsersApiClient> logger;
    
    public UsersApiClient(
        ILogger<UsersApiClient> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptions<ApiClientOptions> options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
        this.logger = logger;
    }
    
    // Get a single user
    public async Task<ApiResponse<UserDto>> GetUserAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
            return await ExecuteWithApiResponseAsync<UserDto>(request, false, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error retrieving user {UserId}", userId);
            var errorResponse = new ApiResponse<UserDto>(new ApiError("InternalError", $"An unexpected error occurred while retrieving user {userId}"));
            return new ApiResult<UserDto>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
    
    // Get a collection of users with filtering
    public async Task<ApiResponse<CollectionModel<UserDto>>> GetUsersAsync(
        FilterOptions filter, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("users", Method.Get, cancellationToken);
            request.AddFilterOptions(filter);
            
            return await ExecuteWithApiResponseAsync<CollectionModel<UserDto>>(
                request, false, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error retrieving users");
            var errorResponse = new ApiResponse<CollectionModel<UserDto>>(new ApiError("InternalError", "An unexpected error occurred while retrieving users"));
            return new ApiResult<CollectionModel<UserDto>>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
    
    // Create a new user
    public async Task<ApiResponse<UserDto>> CreateUserAsync(
        CreateUserDto userDto, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("users", Method.Post, cancellationToken);
            request.AddJsonBody(userDto);
            
            return await ExecuteWithApiResponseAsync<UserDto>(request, false, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error creating user");
            var errorResponse = new ApiResponse<UserDto>(new ApiError("InternalError", "An unexpected error occurred while creating user"));
            return new ApiResult<UserDto>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
    
    // Delete a user (non-data response)
    public async Task<ApiResponse> DeleteUserAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync($"users/{userId}", Method.Delete, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            
            return response.ToApiResponse();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error deleting user with ID {UserId}", userId);
            var errorResponse = new ApiResponse(new ApiError("InternalError", "An unexpected error occurred while deleting user"));
            return new ApiResult(HttpStatusCode.InternalServerError);
        }
    }
}
```

### Working with Request Extensions

```csharp
// Add filter options to a request
var request = await CreateRequestAsync("resources", Method.Get, cancellationToken);
request.AddFilterOptions(new FilterOptions
{
    FilterExpression = "category eq 'books' and price lt 20",
    OrderBy = "title asc",
    Top = 10,
    Skip = 0,
    Expand = new[] { "author", "publisher" },
    Select = new[] { "title", "price", "author.name" }
});

// Add paging parameters
request.AddPagingParameters(pageNumber: 2, pageSize: 20);

// Add query parameters
request.AddQueryParameter("includeDeleted", "true");
```

### Error Handling

```csharp
try
{
    var response = await ExecuteAsync(request, rethrowExceptions: true, cancellationToken);
    // Process successful response and return ApiResult
    return new ApiResult<ResourceDto>
    {
        StatusCode = HttpStatusCode.OK,
        Result = response.Result
    };
}
catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    logger.LogWarning("Resource {Id} not found", id);
    return new ApiResult<ResourceDto>
    {
        StatusCode = HttpStatusCode.NotFound,
        Result = new ApiResponse<ResourceDto>
        {
            Errors = new[] { new ApiError("NotFound", "Resource not found") }
        }
    };
}
catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    logger.LogWarning("Unauthorized access to resource {Id}", id);
    return new ApiResult<ResourceDto>
    {
        StatusCode = HttpStatusCode.Unauthorized,
        Result = new ApiResponse<ResourceDto>
        {
            Errors = new[] { new ApiError("Unauthorized", "Unauthorized") }
        }
    };
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    logger.LogError(ex, "An error occurred while retrieving resource {Id}", id);
    return new ApiResult<ResourceDto>
    {
        StatusCode = HttpStatusCode.InternalServerError,
        Result = new ApiResponse<ResourceDto>
        {
            Errors = new[] { new ApiError("InternalError", "An unexpected error occurred") }
        }
    };
}
```

### API Response Creation Helpers

```csharp
// Create success response wrapped in ApiResult
return new ApiResult<UserDto>
{
    StatusCode = HttpStatusCode.OK,
    Result = new ApiResponse<UserDto>
    {
        Data = user
    }
};

// Create error response wrapped in ApiResult
return new ApiResult<UserDto>
{
    StatusCode = HttpStatusCode.BadRequest,
    Result = new ApiResponse<UserDto>
    {
        Errors = new[]
        {
            new ApiError
            {
                Code = "ValidationError",
                Message = "The provided data is invalid",
                Target = "email",
                Details = new[]
                {
                    new ApiError
                    {
                        Code = "InvalidFormat",
                        Message = "Email format is invalid",
                        Target = "email"
                    }
                }
            }
        }
    }
};
```

> **Note**: The `ApiResult<T>` handles HTTP status codes at the transport layer, while the `ApiResponse<T>` contains the business logic response data and errors.

## Authentication Methods

### API Key Authentication

```csharp
// Add API client with API key authentication
services.AddApiClient()
    .WithApiKeyAuthentication(
        apiKey: "your-api-key",
        headerName: "X-API-Key");  // Optional, defaults to "X-API-Key"
```

### Entra ID (Azure AD) Authentication

```csharp
// Add API client with Entra ID authentication
services.AddApiClient()
    .WithEntraIdAuthentication(
        apiAudience: "api://your-api-audience",
        configureCredentialOptions: options => {
            // Configure DefaultAzureCredentialOptions
            options.ExcludeVisualStudioCredential = true;
            options.ExcludeManagedIdentityCredential = true;
        });
```

### Custom Token Provider

```csharp
// Implement a custom token provider
public class CustomTokenProvider : IApiTokenProvider
{
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        // Custom implementation to retrieve a token
        return await GetTokenFromCustomSourceAsync(cancellationToken);
    }
}

// Register with custom token provider
services.AddApiClient()
    .WithCustomTokenProvider<CustomTokenProvider>();
```

## Resilience and Retry Policies

The library uses Polly for resilience patterns:

```csharp
// Configure retry count
services.Configure<ApiClientOptions>(options =>
{
    options.MaxRetryCount = 3;  // Default is 3
});

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

## License

GPL-3.0-only