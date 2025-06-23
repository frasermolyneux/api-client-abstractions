# API Client

This library provides a base implementation for creating strongly typed API clients with standardized error handling, authentication, and resilience features.

## Features

- Support for multiple authentication methods (API Key and Entra ID)
- Fluent API for easy configuration
- Built-in retry policies with exponential backoff
- Standardized error handling
- Token caching for better performance

## Usage

### Basic Setup

```csharp
// Register the API client services
services.AddApiClient()
    .WithApiKeyAuthentication("your-api-key");

// Or with Entra ID authentication
services.AddApiClient()
    .WithEntraIdAuthentication("api://your-api-audience");

// Inject and use the BaseApi in your custom API client
public class MyApiClient
{
    private readonly BaseApi baseApi;

    public MyApiClient(BaseApi baseApi)
    {
        this.baseApi = baseApi;
    }

    public async Task<MyResource> GetResourceAsync(string id, CancellationToken cancellationToken = default)
    {
        var request = await baseApi.CreateRequestAsync($"resources/{id}", Method.Get, cancellationToken);
        var response = await baseApi.ExecuteAsync(request, cancellationToken);
        
        // Process response
        return JsonConvert.DeserializeObject<MyResource>(response.Content);
    }
}
```

### Updating API Key at Runtime

```csharp
// Inject IOptions<ApiClientOptions> and update
var options = apiClientOptions.Value;
options.UpdateApiKey("new-api-key");
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
