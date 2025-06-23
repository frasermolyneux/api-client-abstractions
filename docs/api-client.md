# MxIO.ApiClient

A generic API client helper library for .NET applications. This package provides a set of base classes and utilities for creating API clients with features including:

- Token-based authentication support
- Configurable retry policies
- Centralized REST client management
- Default Azure AD credential provider integration
- Standardized response handling with ApiResponse<T>
- Unified HTTP response wrapper with HttpResponseWrapper<T>
- Consistent error handling and validation

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

    public async Task<CollectionModel<MyEntity>> GetEntities(FilterOptions filter, CancellationToken cancellationToken = default)
    {
        var request = await _baseApi.CreateRequestAsync("v2/entities", Method.Get, cancellationToken);
        request.AddFilterOptions(filter);
        
        // Option 1: Get typed HttpResponseWrapper with full context
        var wrapper = await _baseApi.ExecuteWithResponseWrapperAsync<CollectionModel<MyEntity>>(request, cancellationToken);
        // Access wrapper.StatusCode, wrapper.IsSuccess, wrapper.Result, etc.
        
        // Option 2: Get just the ApiResponse<T>
        var apiResponse = await _baseApi.ExecuteWithApiResponseAsync<CollectionModel<MyEntity>>(request, cancellationToken);
        // Access apiResponse.Data, apiResponse.Pagination, apiResponse.Errors, etc.
        
        // Option 3: Get just the data directly
        return await _baseApi.ExecuteWithResultAsync<CollectionModel<MyEntity>>(request, cancellationToken);
    }
}
```

## Response Models

The library uses a standard set of response models:

- `ApiResponse<T>`: Contains the data, errors, pagination, metadata, and status code
- `HttpResponseWrapper<T>`: Wraps an ApiResponse with additional HTTP context
- `ApiError`: Standard error format with code, message, target, and details
- `ApiPagination`: Pagination information for collection responses
- `CollectionModel<T>`: Contains a standard list of items for collection endpoints
- `FilterOptions`: Standard filtering and pagination options for queries

## License

GPL-3.0-only