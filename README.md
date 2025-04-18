# API Client Abstractions

[![Code Quality](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml)
[![Feature Development](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml)
[![Pull Request Validation](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml)
[![Release to Production](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml)

## Overview

This repository provides common abstractions and implementations for building robust API clients in .NET applications. The library offers standardized approaches for handling authentication, token management, request execution, and response processing when interacting with REST APIs.

## Libraries

The solution consists of three main packages:

### MxIO.ApiClient

Core library providing the base API client implementation with support for:
- Automatic token acquisition and caching
- Request authentication with API keys
- Primary/secondary API key failover
- Resilient HTTP requests with configurable retry policies
- Thread-safe REST client management

### MxIO.ApiClient.Abstractions

Contains common data transfer object definitions used across API implementations:
- `ApiResponseDto` - Standard API response model
- `CollectionDto` - Collection wrapper for API result sets

### MxIO.ApiClient.WebExtensions

Extension methods for working with API responses in web applications.

## Installation

Install the packages via NuGet:

```bash
dotnet add package MxIO.ApiClient
dotnet add package MxIO.ApiClient.Abstractions
dotnet add package MxIO.ApiClient.WebExtensions
```

## Getting Started

### Basic Setup

Register the API client services in your application's startup:

```csharp
// Add API client with default credential provider
services.AddApiClient();

// Configure client options
services.Configure<ApiClientOptions>(options =>
{
    options.BaseUrl = "https://api.example.com";
    options.ApiPathPrefix = "v1";
    options.PrimaryApiKey = "your-primary-api-key";
    options.SecondaryApiKey = "your-secondary-api-key";
    options.ApiAudience = "api-audience";
    options.MaxRetryCount = 3;
});
```

### Custom Credential Provider

To use a custom credential provider:

```csharp
// Register with custom credential provider
services.AddApiClientWithCustomCredentialProvider<YourCustomCredentialProvider>();

// Add your custom provider implementation
public class YourCustomCredentialProvider : ITokenCredentialProvider
{
    public TokenCredential GetTokenCredential()
    {
        // Your custom implementation
        return new DefaultAzureCredential(...);
    }
}
```

### Creating an API Client

Create your API client by inheriting from `BaseApi`:

```csharp
public class UserApi : BaseApi
{
    public UserApi(
        ILogger<UserApi> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientSingleton restClientSingleton,
        IOptions<ApiClientOptions> options)
        : base(logger, apiTokenProvider, restClientSingleton, options)
    {
    }

    public async Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequest($"users/{userId}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, false, cancellationToken);
        
        return response.IsSuccessful
            ? JsonSerializer.Deserialize<User>(response.Content)
            : null;
    }
}
```

### Using the API Client

Inject and use your API client:

```csharp
public class UserService
{
    private readonly UserApi userApi;

    public UserService(UserApi userApi)
    {
        this.userApi = userApi;
    }

    public async Task<User> GetUserAsync(string userId)
    {
        return await userApi.GetUserAsync(userId);
    }
}
```

## Authentication

The library supports multiple authentication mechanisms:

1. **API Keys**: Provided via `ApiClientOptions.PrimaryApiKey` and `ApiClientOptions.SecondaryApiKey`
2. **Bearer Tokens**: Automatically managed and cached by `ApiTokenProvider`
3. **Azure Authentication**: Leverages `DefaultAzureCredential` for Azure resource access

The `DefaultTokenCredentialProvider` attempts to authenticate via:
- Environment variables
- Managed Identity
- Visual Studio
- Azure CLI
- Azure PowerShell

## Error Handling and Resilience

The `BaseApi` class implements automatic retries with exponential backoff for transient failures. You can configure retry behavior through `ApiClientOptions.MaxRetryCount`.

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
