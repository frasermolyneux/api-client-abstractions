# MX API Abstractions

[![Code Quality](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml)
[![Feature Development](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml)
[![Pull Request Validation](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml)
[![Release to Production](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml)

## Overview

This repository provides a comprehensive API Abstractions approach for building robust .NET applications that interact with APIs. The libraries offer standardized approaches for API interactions from both client and web perspectives, including common abstractions, API client implementations, and web application integration.

The implementation follows the API design pattern, which provides a consistent approach to API interactions with standardized response formats, error handling, filtering, and pagination across all API touchpoints in your applications.

## Libraries

The solution consists of three main packages that work together to provide a complete API abstractions approach:

### MX.Api.Abstractions

Core abstractions library providing common models and interfaces for standardized API handling:
- `ApiResponse` - Standard API response model for operations without data (e.g., DELETE operations)
- `ApiResponse<T>` - Standard API response model for operations with data following API design
- `ApiResult<T>` - HTTP response wrapper containing API responses
- `CollectionModel<T>` - Collection wrapper for API result sets
- `ApiError` - Standardized error model
- `ApiPagination` - Pagination information
- `FilterOptions` - OData-like filtering options

### MX.Api.Client

API client library providing resilient, authenticated REST API client implementation:
- Automatic token acquisition and caching
- Request authentication with API keys or Entra ID (formerly Azure AD)
- API key authentication with resilient handling
- Resilient HTTP requests with configurable retry policies
- Thread-safe REST client management
- Support for common query parameters and filtering options

### MX.Api.Web.Extensions

ASP.NET Core integration library providing extension methods for web applications:
- Converting API responses to ActionResults with appropriate status codes
- Simplified controller implementations for API endpoints
- Seamless integration between API clients and web applications

## Architecture

The MX API Abstractions approach is designed as a layered architecture:

1. **MX.Api.Abstractions** - Foundation layer providing core models and interfaces
2. **MX.Api.Client** - Implementation layer for API client functionality (depends on Abstractions)
3. **MX.Api.Web.Extensions** - Integration layer for ASP.NET Core applications (depends on Abstractions)

This layered approach ensures that:
- Core abstractions remain stable and reusable across different implementations
- API clients can be built with consistent patterns and standards
- Web applications can seamlessly integrate with API clients
- All components follow the same API design principles

## Installation

Install the packages via NuGet based on your needs:

```bash
# For core abstractions (required by other packages)
dotnet add package MX.Api.Abstractions

# For building API clients
dotnet add package MX.Api.Client

# For ASP.NET Core web application integration
dotnet add package MX.Api.Web.Extensions
```

### Package Dependencies

- **MX.Api.Abstractions** - Standalone core abstractions (no dependencies on other MX packages)
- **MX.Api.Client** - Depends on MX.Api.Abstractions
- **MX.Api.Web.Extensions** - Depends on MX.Api.Abstractions (independent of MX.Api.Client)

You can use any combination of these packages based on your application's needs. For example:
- Use only MX.Api.Abstractions if you're building your own API client implementation
- Use MX.Api.Abstractions + MX.Api.Client for consuming APIs in console/service applications
- Use all three packages for full-stack web applications that both consume and expose APIs

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
            var errorResponse = new ApiResponse<User>(new ApiError("InternalError", "An unexpected error occurred while retrieving the user"));
            return new ApiResult<User>(HttpStatusCode.InternalServerError, errorResponse);
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
            return response.Result.Data;
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

1. **API Keys**: Provided via `ApiClientOptions.WithApiKeyAuthentication(apiKey)` or `AuthenticationOptions.ApiKey`
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

This library implements the API design pattern, which provides:

- Standardized response formats with consistent error handling across all API interactions
- OData-like filtering with `$filter`, `$select`, `$expand`, etc. for flexible data queries
- Pagination support with skip/take and comprehensive metadata
- Consistent URL structure and query parameters following RESTful principles
- Unified approach to API interactions whether consuming or exposing APIs

For more details on the API design pattern, see the [API Design Documentation](docs/api-design-v2.md).

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project focused on building robust API abstraction patterns for .NET applications.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
