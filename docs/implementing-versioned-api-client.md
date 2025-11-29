# Implementing Versioned API Clients

This guide demonstrates how to implement API clients that support multiple versions of an API using the versioned API pattern. This pattern provides a clean, extensible way to handle API evolution while maintaining backward compatibility.

## Overview

The versioned API pattern allows you to organize your API client around different API versions (V1, V2, etc.) while providing a unified interface for consumers. This approach is particularly useful when:

- Your API has multiple versions in production
- You need to support gradual migration between API versions
- Different features are available across different API versions
- You want to provide a future-proof client architecture

### Package Versioning & Multi-targeting

- **Framework support** – All MX.Api packages now multi-target `net9.0` and `net10.0`, so you can adopt the next .NET wave (including previews) without changing dependencies.
- **Preview builds** – Every non-tag build published from `feature/*` branches lands on NuGet with a `-preview` suffix so you can validate changes early in lower environments.
- **Stable releases** – Create a Git tag that starts with `v` (for example `v2.3.0` or `v2.3.0-preview.2`) to trigger the release workflow; the tag text becomes the NuGet version applied across all packages.
- **Automation** – The shared GitHub Actions install each requested SDK and build/test every target framework before uploading artifacts, ensuring parity between preview and production releases.

## Architecture

The versioned API pattern consists of several layers:

```
UserApiClient (Main Client)
├── IVersionedUserApi (Version Selector)
│   ├── V1 (IUserApiV1) → UserApiV1 implementation
│   ├── V2 (IUserApiV2) → UserApiV2 implementation  
│   └── V3 (IUserApiV3) → UserApiV3 implementation
└── Shared Options and Configuration
```

## Implementation Steps

### 1. Define Version-Specific Interfaces

First, create interfaces for each API version:

```csharp
// V1 Interface
namespace MyCompany.UserApi.Client.Interfaces.V1;

public interface IUserApiV1
{
    Task<ApiResult<CollectionModel<User>>> GetUsersAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    
    Task<ApiResult<User>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResult<User>> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<ApiResult<string>> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
}

// V2 Interface (with enhanced features)
namespace MyCompany.UserApi.Client.Interfaces.V2;

public interface IUserApiV2
{
    // All V1 methods plus new features
    Task<ApiResult<CollectionModel<User>>> GetUsersAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        UserSortOrder sortOrder = UserSortOrder.Name,
        CancellationToken cancellationToken = default);
    
    Task<ApiResult<User>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResult<User>> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<ApiResult<User>> UpdateUserAsync(int userId, User user, CancellationToken cancellationToken = default);
    Task<ApiResult<string>> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
    
    // V2-specific features
    Task<ApiResult<UserProfile>> GetUserProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResult<CollectionModel<User>>> SearchUsersAsync(UserSearchRequest request, CancellationToken cancellationToken = default);
}
```

### 2. Implement Version-Specific API Classes

Create concrete implementations for each version:

```csharp
// V1 Implementation
namespace MyCompany.UserApi.Client.Api.V1;

public class UserApiV1 : BaseApi<UserApiOptions>, IUserApiV1
{
    public UserApiV1(
        ILogger<BaseApi<UserApiOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        UserApiOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<CollectionModel<User>>> GetUsersAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v1/users?page={page}&pageSize={pageSize}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<CollectionModel<User>>();
    }

    public async Task<ApiResult<User>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v1/users/{userId}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<User>();
    }

    public async Task<ApiResult<User>> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("v1/users", Method.Post, cancellationToken);
        request.AddJsonBody(user);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<User>();
    }

    public async Task<ApiResult<string>> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v1/users/{userId}", Method.Delete, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<string>();
    }
}

// V2 Implementation
namespace MyCompany.UserApi.Client.Api.V2;

public class UserApiV2 : BaseApi<UserApiOptions>, IUserApiV2
{
    public UserApiV2(
        ILogger<BaseApi<UserApiOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        UserApiOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<CollectionModel<User>>> GetUsersAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        UserSortOrder sortOrder = UserSortOrder.Name,
        CancellationToken cancellationToken = default)
    {
        var queryParams = $"page={page}&pageSize={pageSize}&sortOrder={sortOrder}";
        if (!string.IsNullOrEmpty(searchTerm))
            queryParams += $"&search={Uri.EscapeDataString(searchTerm)}";
            
        var request = await CreateRequestAsync($"v2/users?{queryParams}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<CollectionModel<User>>();
    }

    // Implement other methods...
    
    public async Task<ApiResult<UserProfile>> GetUserProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v2/users/{userId}/profile", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<UserProfile>();
    }

    public async Task<ApiResult<CollectionModel<User>>> SearchUsersAsync(UserSearchRequest request, CancellationToken cancellationToken = default)
    {
        var apiRequest = await CreateRequestAsync("v2/users/search", Method.Post, cancellationToken);
        apiRequest.AddJsonBody(request);
        var response = await ExecuteAsync(apiRequest, cancellationToken);
        return response.ToApiResult<CollectionModel<User>>();
    }
}
```

### 3. Create the Versioned API Interface

Define an interface that exposes all available versions:

```csharp
namespace MyCompany.UserApi.Client;

/// <summary>
/// Interface for versioned User API client providing access to all API versions
/// </summary>
public interface IVersionedUserApi
{
    /// <summary>
    /// Gets the V1 User API
    /// </summary>
    IUserApiV1 V1 { get; }
    
    /// <summary>
    /// Gets the V2 User API
    /// </summary>
    IUserApiV2 V2 { get; }
}
```

### 4. Implement the Versioned API Wrapper

Create a concrete implementation that aggregates all versions:

```csharp
namespace MyCompany.UserApi.Client;

/// <summary>
/// Implementation of versioned User API client
/// </summary>
public class VersionedUserApi : IVersionedUserApi
{
    public VersionedUserApi(IUserApiV1 v1Api, IUserApiV2 v2Api)
    {
        V1 = v1Api;
        V2 = v2Api;
    }

    /// <summary>
    /// Gets the V1 User API
    /// </summary>
    public IUserApiV1 V1 { get; }
    
    /// <summary>
    /// Gets the V2 User API
    /// </summary>
    public IUserApiV2 V2 { get; }
}
```

### 5. Create the Main Client Interface

Define the primary client interface that consumers will use:

```csharp
namespace MyCompany.UserApi.Client;

/// <summary>
/// Interface for the User API client providing access to versioned APIs
/// </summary>
public interface IUserApiClient
{
    /// <summary>
    /// Gets the versioned User API providing access to all available versions
    /// </summary>
    IVersionedUserApi Users { get; }
}
```

### 6. Implement the Main Client

Create the main client implementation:

```csharp
namespace MyCompany.UserApi.Client;

/// <summary>
/// User API client implementation using versioned approach
/// </summary>
public class UserApiClient : IUserApiClient
{
    /// <summary>
    /// Initializes a new instance of the UserApiClient
    /// </summary>
    public UserApiClient(IVersionedUserApi versionedUserApi)
    {
        Users = versionedUserApi;
    }

    /// <summary>
    /// Gets the versioned User API providing access to all available versions
    /// </summary>
    public IVersionedUserApi Users { get; }
}
```

### 7. Create Custom Options (Optional)

If you need custom configuration, create custom options:

```csharp
namespace MyCompany.UserApi.Client;

/// <summary>
/// Custom options for the User API client
/// </summary>
public class UserApiOptions : ApiClientOptionsBase
{
    /// <summary>
    /// Gets or sets whether user caching is enabled
    /// </summary>
    public bool EnableUserCaching { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the cache expiration time in minutes
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 15;
    
    /// <summary>
    /// Gets or sets whether detailed logging is enabled
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the default user role for new users
    /// </summary>
    public string DefaultUserRole { get; set; } = "Member";
    
    /// <summary>
    /// Gets or sets the maximum page size for user queries
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Validates the options
    /// </summary>
    public override void Validate()
    {
        base.Validate();
        
        if (CacheExpirationMinutes < 1)
            throw new InvalidOperationException("CacheExpirationMinutes must be at least 1");
            
        if (MaxPageSize < 1 || MaxPageSize > 1000)
            throw new InvalidOperationException("MaxPageSize must be between 1 and 1000");
            
        if (string.IsNullOrEmpty(DefaultUserRole))
            throw new InvalidOperationException("DefaultUserRole cannot be null or empty");
    }
}

/// <summary>
/// Builder for User API options
/// </summary>
public class UserApiOptionsBuilder : ApiClientOptionsBuilder<UserApiOptions, UserApiOptionsBuilder>
{
    /// <summary>
    /// Enables user caching with the specified expiration time
    /// </summary>
    public UserApiOptionsBuilder WithUserCaching(int expirationMinutes)
    {
        Options.EnableUserCaching = true;
        Options.CacheExpirationMinutes = expirationMinutes;
        return this;
    }
    
    /// <summary>
    /// Enables detailed logging for debugging
    /// </summary>
    public UserApiOptionsBuilder WithDetailedLogging()
    {
        Options.EnableDetailedLogging = true;
        return this;
    }
    
    /// <summary>
    /// Sets the default role for new users
    /// </summary>
    public UserApiOptionsBuilder WithDefaultRole(string role)
    {
        Options.DefaultUserRole = role;
        return this;
    }
    
    /// <summary>
    /// Sets the maximum page size for queries
    /// </summary>
    public UserApiOptionsBuilder WithMaxPageSize(int maxPageSize)
    {
        Options.MaxPageSize = maxPageSize;
        return this;
    }
}
```

### 8. Create Service Collection Extensions

Provide easy registration methods for dependency injection:

```csharp
namespace MyCompany.UserApi.Client;

/// <summary>
/// Extension methods for registering the User API client
/// </summary>
public static class UserApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds the User API client with simplified configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="baseUrl">The base URL of the User API</param>
    /// <param name="apiToken">The API token for authentication</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUserApiClient(
        this IServiceCollection services,
        string baseUrl,
        string apiToken)
    {
        // Register V1 API
        services.AddTypedApiClient<IUserApiV1, UserApiV1, UserApiOptions, UserApiOptionsBuilder>(builder => builder
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiToken)
            .WithUserCaching(30)
            .WithDetailedLogging()
            .WithDefaultRole("Member")
            .WithMaxPageSize(50));

        // Register V2 API (shares same options but different implementation)
        services.AddTypedApiClient<IUserApiV2, UserApiV2, UserApiOptions, UserApiOptionsBuilder>(builder => builder
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiToken)
            .WithUserCaching(30)
            .WithDetailedLogging()
            .WithDefaultRole("Member")
            .WithMaxPageSize(50));

        // Register versioned API wrapper
        services.AddScoped<IVersionedUserApi, VersionedUserApi>();

        // Register main client
        services.AddScoped<IUserApiClient, UserApiClient>();

        return services;
    }

    /// <summary>
    /// Adds the User API client with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the User API options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUserApiClient(
        this IServiceCollection services,
        Action<UserApiOptionsBuilder> configureOptions)
    {
        // Register V1 API
        services.AddTypedApiClient<IUserApiV1, UserApiV1, UserApiOptions, UserApiOptionsBuilder>(configureOptions);

        // Register V2 API with same configuration
        services.AddTypedApiClient<IUserApiV2, UserApiV2, UserApiOptions, UserApiOptionsBuilder>(configureOptions);

        // Register versioned API wrapper
        services.AddScoped<IVersionedUserApi, VersionedUserApi>();

        // Register main client
        services.AddScoped<IUserApiClient, UserApiClient>();

        return services;
    }
}
```

## Usage Examples

### Basic Registration

```csharp
// Program.cs
builder.Services.AddUserApiClient(
    "https://api.example.com",
    "your-api-key");
```

### Advanced Registration

```csharp
// Program.cs
builder.Services.AddUserApiClient(options => options
    .WithBaseUrl("https://api.example.com")
    .WithApiKeyAuthentication("your-api-key")
    .WithUserCaching(60)
    .WithDetailedLogging()
    .WithDefaultRole("Premium")
    .WithMaxPageSize(100));
```

### Using the Versioned Client

```csharp
public class UserService
{
    private readonly IUserApiClient _userApiClient;

    public UserService(IUserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        // Use V2 API for enhanced features
        var result = await _userApiClient.Users.V2.GetUserByIdAsync(userId);
        
        if (result.IsSuccess)
            return result.Data;
            
        // Fallback to V1 if V2 fails
        var v1Result = await _userApiClient.Users.V1.GetUserByIdAsync(userId);
        return v1Result.IsSuccess ? v1Result.Data : null;
    }

    public async Task<CollectionModel<User>> SearchUsersAsync(string searchTerm)
    {
        // Use V2-specific search functionality
        var searchRequest = new UserSearchRequest 
        { 
            SearchTerm = searchTerm,
            IncludeInactive = false,
            SortOrder = UserSortOrder.LastActivity
        };
        
        var result = await _userApiClient.Users.V2.SearchUsersAsync(searchRequest);
        return result.IsSuccess ? result.Data : new CollectionModel<User>();
    }

    public async Task<CollectionModel<User>> GetUsersLegacyAsync(int page, int pageSize)
    {
        // Use V1 API for backward compatibility
        var result = await _userApiClient.Users.V1.GetUsersAsync(page, pageSize);
        return result.IsSuccess ? result.Data : new CollectionModel<User>();
    }
}
```

## Project Structure

Organize your versioned API client project as follows:

```
MyCompany.UserApi.Client/
├── Api/
│   ├── V1/
│   │   └── UserApiV1.cs
│   └── V2/
│       └── UserApiV2.cs
├── Interfaces/
│   ├── V1/
│   │   └── IUserApiV1.cs
│   └── V2/
│       └── IUserApiV2.cs
├── Models/
│   ├── V1/
│   │   └── User.cs
│   ├── V2/
│   │   ├── User.cs
│   │   ├── UserProfile.cs
│   │   └── UserSearchRequest.cs
│   └── Shared/
│       └── UserSortOrder.cs
├── IUserApiClient.cs
├── IVersionedUserApi.cs
├── UserApiClient.cs
├── UserApiOptions.cs
├── UserApiOptionsBuilder.cs
├── UserApiServiceCollectionExtensions.cs
└── VersionedUserApi.cs
```

## Benefits

The versioned API pattern provides several advantages:

1. **Version Isolation**: Each API version has its own interface and implementation
2. **Future-Proof**: Easy to add new versions without breaking existing code
3. **Gradual Migration**: Consumers can migrate between versions at their own pace
4. **Feature Detection**: Clear visibility into which features are available in each version
5. **Backward Compatibility**: Maintain support for older API versions
6. **Testability**: Each version can be mocked and tested independently

## Best Practices

1. **Shared Options**: Use the same options class across versions for consistent configuration
2. **Version Naming**: Use clear, semantic version names (V1, V2, etc.)
3. **Interface Evolution**: When adding new versions, consider which methods to carry forward
4. **Error Handling**: Implement consistent error handling patterns across all versions
5. **Documentation**: Document the differences between versions and migration paths
6. **Testing**: Create comprehensive tests for each version and the versioned wrapper
7. **Deprecation**: Clearly document when older versions will be deprecated

This pattern provides a robust foundation for building API clients that can evolve with your APIs while maintaining stability for consumers.
