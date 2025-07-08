# MX API Abstractions

[![Code Quality](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/codequality.yml)
[![Feature Development](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/feature-development.yml)
[![Pull Request Validation](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/pull-request-validation.yml)
[![Release to Production](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml/badge.svg)](https://github.com/frasermolyneux/api-client-abstractions/actions/workflows/release-to-production.yml)

A comprehensive set of .NET libraries providing standardized API abstractions, resilient API clients, and ASP.NET Core integration for building robust applications that consume or expose APIs.

## Quick Start

### Installation

Install the packages you need via NuGet:

```bash
# Core abstractions (required by other packages)
dotnet add package MX.Api.Abstractions

# For building API clients
dotnet add package MX.Api.Client

# For ASP.NET Core web application integration  
dotnet add package MX.Api.Web.Extensions
```

### API Client Registration

**Option 1: Simple Registration (Recommended for most cases)**

```csharp
// Program.cs
builder.Services.AddApiClient<IMyApiClient, MyApiClient>(options =>
{
    options.WithBaseUrl("https://api.example.com")
           .WithApiKeyAuthentication("your-api-key");
});

// Define your client interface
public interface IMyApiClient
{
    Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default);
}

// Implement your client
public class MyApiClient : BaseApi, IMyApiClient
{
    public MyApiClient(
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
}
```

**Option 2: Advanced Registration (For complex scenarios)**

```csharp
// Define custom options for advanced scenarios
public class MyApiClientOptions : ApiClientOptionsBase
{
    public string ApiVersion { get; set; } = "v1";
    public int CacheTimeoutMinutes { get; set; } = 5;
}

public class MyApiClientOptionsBuilder : ApiClientOptionsBuilder<MyApiClientOptions, MyApiClientOptionsBuilder>
{
    public MyApiClientOptionsBuilder WithApiVersion(string version)
    {
        Options.ApiVersion = version;
        return this;
    }
}

// Register with strongly-typed options
builder.Services.AddTypedApiClient<IMyApiClient, MyApiClient, MyApiClientOptions, MyApiClientOptionsBuilder>(options =>
{
    options.WithBaseUrl("https://api.example.com")
           .WithApiKeyAuthentication("your-api-key")
           .WithApiVersion("v2");
});

// Use strongly-typed options in your client
public class MyApiClient : BaseApi<MyApiClientOptions>, IMyApiClient
{
    public MyApiClient(
        ILogger<BaseApi<MyApiClientOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        MyApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"users/{userId}", Method.Get, cancellationToken);
        
        // Access your custom options
        request.AddQueryParameter("version", Options.ApiVersion);
        
        var response = await ExecuteAsync(request, false, cancellationToken);
        return response.ToApiResponse<User>();
    }
}
```

### Using Your Client

```csharp
public class UserService
{
    private readonly IMyApiClient _apiClient;
    
    public UserService(IMyApiClient apiClient) // Use interface for better testability
    {
        _apiClient = apiClient;
    }
    
    public async Task<User?> GetUserAsync(string userId)
    {
        var result = await _apiClient.GetUserAsync(userId);
        return result.IsSuccess ? result.Result?.Data : null;
    }
}
```

## Libraries

This solution consists of three focused NuGet packages:

| Package                   | Purpose                                                      | Dependencies        |
| ------------------------- | ------------------------------------------------------------ | ------------------- |
| **MX.Api.Abstractions**   | Core models and interfaces for standardized API handling     | None                |
| **MX.Api.Client**         | Resilient REST API client implementation with authentication | MX.Api.Abstractions |
| **MX.Api.Web.Extensions** | ASP.NET Core integration for API responses                   | MX.Api.Abstractions |

## Key Features

- **🔐 Multi-Authentication Support** - API keys, Bearer tokens, and Entra ID with automatic token management
- **🛡️ Resilience Built-In** - Retry policies, circuit breakers, and exponential backoff
- **📐 Standardized Responses** - Consistent API response models following proven design patterns
- **🔄 ASP.NET Core Integration** - Seamless conversion between API responses and HTTP results
- **⚡ High Performance** - Thread-safe operations with efficient caching and connection pooling

## Documentation

- **[📖 Implementation Guide - API Providers](docs/implementing-api-provider.md)** - Guide for teams building APIs
- **[📖 Implementation Guide - API Consumers](docs/implementing-api-consumer.md)** - Guide for teams consuming APIs  
- **[� Versioned API Client Pattern](docs/implementing-versioned-api-client.md)** - Advanced pattern for multi-version API support
- **[�📐 API Design Patterns](docs/api-design-v2.md)** - Understanding the underlying design principles

## Authentication

Supports multiple authentication methods that can be combined:

```csharp
// API Key only
services.AddApiClient()
    .WithApiKeyAuthentication("your-api-key");

// Entra ID only  
services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience");

// Combined (e.g., Azure API Management + Backend API)
services.Configure<ApiClientOptions>(options => options
    .WithSubscriptionKey("apim-subscription-key")     // For APIM
    .WithEntraIdAuthentication("api://backend-api")); // For backend
```

## Contributing

Please read the [contributing guidelines](CONTRIBUTING.md) for information about contributing to this project. This is a learning and development project focused on building robust API abstraction patterns for .NET applications.

## Security

Please read the [security policy](SECURITY.md) for information about reporting security vulnerabilities. I am always open to security feedback through email or opening an issue.

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.
