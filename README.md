# API Client Abstractions

[![Code Quality](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml)
[![Feature Development](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml)
[![Pull Request Validation](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml)
[![Release to Production](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml)

## Overview

This repository provides a comprehensive toolkit for building robust .NET API clients. The library offers standardized approaches for handling authentication, token management, request execution, and response processing when interacting with REST APIs.

The implementation follows the v2 API design pattern, which provides a consistent approach to API interactions with standardized response formats, error handling, filtering, and pagination.

## Libraries

The solution consists of three main packages:

### MxIO.ApiClient

Core library providing resilient, authenticated REST API client implementation with:
- Automatic token acquisition and caching
- Request authentication with API keys or Entra ID (formerly Azure AD)
- Primary/secondary API key failover
- Resilient HTTP requests with configurable retry policies
- Thread-safe REST client management
- Support for common query parameters and filtering options

### MxIO.ApiClient.Abstractions.V2

Common models and interfaces for standardized API response handling:
- `ApiResponse<T>` - Standard API response model following v2 API design
- `HttpResponseWrapper<T>` - HTTP response wrapper containing API responses
- `CollectionModel<T>` - Collection wrapper for API result sets
- `ApiError` - Standardized error model
- `ApiPagination` - Pagination information
- `FilterOptions` - OData-like filtering options

### MxIO.ApiClient.WebExtensions.V2

Extension methods for integrating with ASP.NET Core web applications:
- Converting API responses to ActionResults with appropriate status codes
- Simplified controller implementations for API endpoints

## Installation

Install the packages via NuGet:

```bash
dotnet add package MxIO.ApiClient
dotnet add package MxIO.ApiClient.Abstractions.V2
dotnet add package MxIO.ApiClient.WebExtensions.V2
```

## Getting Started

### Basic Setup

Register the API client services in your application's startup:

```csharp
// Add API client with default credential provider
services.AddApiClient()
    .WithApiKeyAuthentication("your-api-key");

// Or with Entra ID authentication
services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience");
```

### Custom Credential Provider

To use a custom credential provider:

```csharp
// Register with custom credential provider
services.AddApiClientWithCustomCredentialProvider<YourCustomCredentialProvider>();

// Add your custom provider implementation
public class YourCustomCredentialProvider : ITokenCredentialProvider
{
    public Task<TokenCredential> GetTokenCredentialAsync(CancellationToken cancellationToken = default)
    {
        // Your custom implementation
        return Task.FromResult<TokenCredential>(new DefaultAzureCredential(cancellationToken: cancellationToken));
    }
}
```

### Creating an API Client

Create your API client by inheriting from `BaseApi`:

```csharp
public class UserApi : BaseApi
{
    private readonly ILogger<UserApi> logger;

    public UserApi(
        ILogger<UserApi> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptions<ApiClientOptions> options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
        this.logger = logger;
    }

    public async Task<ApiResponse<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving user with ID {UserId}", userId);
            
            var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            
            return response.ToApiResponse<User>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to retrieve user with ID {UserId}", userId);
            return HttpStatusCode.InternalServerError.CreateResponse<User>("An unexpected error occurred while retrieving the user");
        }
    }
}
```

### Using the API Client

Inject and use your API client:

```csharp
public class UserService
{
    private readonly ILogger<UserService> logger;
    private readonly UserApi userApi;

    public UserService(ILogger<UserService> logger, UserApi userApi)
    {
        this.logger = logger;
        this.userApi = userApi;
    }

    public async Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await userApi.GetUserAsync(userId, cancellationToken);
        
        if (response.IsSuccess && response.Result != null)
        {
            return response.Result;
        }
        
        if (response.IsNotFound)
        {
            logger.LogWarning("User with ID {UserId} was not found", userId);
            return null;
        }
        
        logger.LogError("Failed to retrieve user with ID {UserId}. Status code: {StatusCode}", 
            userId, response.StatusCode);
        return null;
    }
}
```

## Authentication

The library supports multiple authentication mechanisms:

1. **API Keys**: Provided via `ApiClientOptions.PrimaryApiKey` and `ApiClientOptions.SecondaryApiKey`
2. **Bearer Tokens**: Automatically managed and cached by `ApiTokenProvider`
3. **Entra ID (formerly Azure AD)**: Leverages `DefaultAzureCredential` for Azure resource access

The `DefaultTokenCredentialProvider` attempts to authenticate via:
- Environment variables
- Managed Identity
- Visual Studio
- Azure CLI
- Azure PowerShell

## Error Handling and Resilience

The `BaseApi` class implements automatic retries with exponential backoff for transient failures. You can configure retry behavior through `ApiClientOptions.MaxRetryCount`.

## API Design Pattern

This library implements the v2 API design pattern, which provides:

- Standardized response formats with consistent error handling
- OData-like filtering with `$filter`, `$select`, `$expand`, etc.
- Pagination support with skip/take and metadata
- Consistent URL structure and query parameters

For more details on the v2 API design pattern, see the [API Design V2 Documentation](docs/api-design-v2.md).

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
