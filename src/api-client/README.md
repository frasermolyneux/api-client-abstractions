# MxIO.ApiClient

A generic API client helper library for .NET applications. This package provides a set of base classes and utilities for creating API clients with features including:

- Token-based authentication support
- Configurable retry policies
- Centralized REST client management
- Default Azure AD credential provider integration

## Installation

```
dotnet add package MxIO.ApiClient
```

## Basic Usage

```csharp
// Add API client services to your DI container
services.AddApiClient(Configuration);

// Inject and use the base API in your services
public class MyService
{
    private readonly BaseApi _baseApi;

    public MyService(BaseApi baseApi)
    {
        _baseApi = baseApi;
    }

    public async Task DoSomething()
    {
        // Use the API client
    }
}
```

## License

GPL-3.0-only