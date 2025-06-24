# MxIO.ApiClient.Abstractions.V2

A library containing common abstractions and models for API clients in .NET applications. This package provides the fundamental data structures and interfaces for building API clients that conform to the v2 API design.

## Features

- API Response Models following the v2 API design pattern
- Collection Models for standardized result sets
- HTTP Response Wrappers to separate HTTP concerns from API response models
- Filter Options for OData-like query parameters
- Pagination support with metadata

## Installation

```
dotnet add package MxIO.ApiClient.Abstractions.V2
```

## API Response Model Structure

The library implements a clear separation between HTTP-level concerns and the API response model:

### API Response Model

```csharp
// The standard API response model
public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public ApiError[]? Errors { get; set; }
    public ApiPagination? Pagination { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

### HTTP Response Wrapper

```csharp
// Wrapper that handles HTTP response concerns
public class HttpResponseWrapper<T>
{
    public ApiResponse<T>? Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; }
    public bool IsNotFound { get; }
    public bool IsConflict { get; }
}
```

## Collection Model

For endpoints that return collections:

```csharp
public class CollectionModel<T>
{
    public List<T> Items { get; set; } = new List<T>();
}
```

## Basic Usage

```csharp
// Example of using the HttpResponseWrapper with ApiResponse
HttpResponseWrapper<MyData> responseWrapper = await apiClient.GetDataAsync();

if (responseWrapper.IsSuccess && responseWrapper.Result != null)
{
    MyData data = responseWrapper.Result.Data;
    // Process data
}
else
{
    // Handle error
    var errors = responseWrapper.Result?.Errors;
}

// Example of working with collections
HttpResponseWrapper<CollectionModel<MyItem>> collectionWrapper = await apiClient.GetCollectionAsync();
if (collectionWrapper.IsSuccess && collectionWrapper.Result?.Data != null)
{
    foreach (var item in collectionWrapper.Result.Data.Items)
    {
        // Process each item
    }
    
    // Access pagination information
    var pagination = collectionWrapper.Result.Pagination;
    int totalCount = pagination?.TotalCount ?? 0;
}
```

## Common Query Parameters

All collection endpoints support the following query parameters:

| Parameter  | Description                                           |
| ---------- | ----------------------------------------------------- |
| `$filter`  | OData-like filter expression                          |
| `$select`  | Select specific fields                                |
| `$expand`  | Expand related entities                               |
| `$orderby` | Sort by field(s)                                      |
| `$top`     | Number of records to take                             |
| `$skip`    | Number of records to skip                             |
| `$count`   | When true, returns only the count of matching records |

## License

GPL-3.0-only