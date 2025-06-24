# MxIO.ApiClient.Abstractions.V2

A library containing common abstractions and models for API clients in .NET applications. This package provides the fundamental data structures and interfaces for building API clients that conform to the v2 API design.

## Features

- API Response Models following the v2 API design pattern
- Collection Models for standardized result sets
- HTTP Response Wrappers to separate HTTP concerns from API response models
- Filter Options for OData-like query parameters
- Pagination support with metadata
- Standardized error models

## Installation

```bash
dotnet add package MxIO.ApiClient.Abstractions.V2
```

## Core Components

### API Response Model

The library implements a clear separation between HTTP-level concerns and the API response model:

```csharp
// The standard API response model
public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public ApiError[]? Errors { get; set; }
    public ApiPagination? Pagination { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    
    // Helper properties
    public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
    public bool IsConflict => StatusCode == HttpStatusCode.Conflict;
    public bool IsBadRequest => StatusCode == HttpStatusCode.BadRequest;
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
    public bool IsForbidden => StatusCode == HttpStatusCode.Forbidden;
    public bool IsInternalServerError => StatusCode == HttpStatusCode.InternalServerError;
}
```

### HTTP Response Wrapper

```csharp
// Wrapper that handles HTTP response concerns
public class HttpResponseWrapper<T>
{
    public ApiResponse<T>? Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? Content { get; set; }
    
    // Helper properties
    public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
    public bool IsConflict => StatusCode == HttpStatusCode.Conflict;
    public bool IsBadRequest => StatusCode == HttpStatusCode.BadRequest;
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
    public bool IsForbidden => StatusCode == HttpStatusCode.Forbidden;
    public bool IsInternalServerError => StatusCode == HttpStatusCode.InternalServerError;
}
```

### Error Model

```csharp
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Target { get; set; }
    public ApiError[]? Details { get; set; }
}
```

### Collection Model

For endpoints that return collections:

```csharp
public class CollectionModel<T>
{
    public List<T> Items { get; set; } = new List<T>();
}
```

### Pagination Model

```csharp
public class ApiPagination
{
    public int TotalCount { get; set; }
    public int FilteredCount { get; set; }
    public int Skip { get; set; }
    public int Top { get; set; }
    public bool HasMore { get; set; }
}
```

### Filter Options

```csharp
public class FilterOptions
{
    public string? FilterExpression { get; set; }
    public string[]? Select { get; set; }
    public string[]? Expand { get; set; }
    public string? OrderBy { get; set; }
    public int Skip { get; set; }
    public int Top { get; set; }
    public bool Count { get; set; }
}
```

## Usage Examples

### Working with API Responses

```csharp
// Example of using the HttpResponseWrapper with ApiResponse
HttpResponseWrapper<UserDto> responseWrapper = await apiClient.GetUserAsync("123");

if (responseWrapper.IsSuccess && responseWrapper.Result?.Data != null)
{
    UserDto user = responseWrapper.Result.Data;
    // Process user data
}
else if (responseWrapper.IsNotFound)
{
    // Handle not found
}
else
{
    // Handle error
    var errors = responseWrapper.Result?.Errors;
    foreach (var error in errors ?? Array.Empty<ApiError>())
    {
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
    }
}
```

### Working with Collections

```csharp
// Example of working with collections
HttpResponseWrapper<CollectionModel<UserDto>> collectionWrapper = await apiClient.GetUsersAsync(new FilterOptions 
{
    FilterExpression = "active eq true",
    OrderBy = "lastName asc",
    Skip = 0,
    Top = 10
});

if (collectionWrapper.IsSuccess && collectionWrapper.Result?.Data != null)
{
    foreach (var user in collectionWrapper.Result.Data.Items)
    {
        // Process each user
    }
    
    // Access pagination information
    var pagination = collectionWrapper.Result.Pagination;
    int totalCount = pagination?.TotalCount ?? 0;
    int filteredCount = pagination?.FilteredCount ?? 0;
    bool hasMore = pagination?.HasMore ?? false;
    
    if (hasMore)
    {
        // Load more items
    }
}
```

### Creating API Responses

```csharp
// Creating a successful response
ApiResponse<UserDto> successResponse = new ApiResponse<UserDto>
{
    StatusCode = HttpStatusCode.OK,
    Data = new UserDto { Id = "123", Name = "John Doe" }
};

// Creating an error response
ApiResponse<UserDto> errorResponse = new ApiResponse<UserDto>
{
    StatusCode = HttpStatusCode.BadRequest,
    Errors = new[]
    {
        new ApiError
        {
            Code = "ValidationError",
            Message = "The user data is invalid",
            Target = "name",
            Details = new[]
            {
                new ApiError
                {
                    Code = "Required",
                    Message = "Name is required",
                    Target = "name"
                }
            }
        }
    }
};
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

## Best Practices

1. **Consistent Error Handling**: Use the ApiError model to provide detailed error information in all API responses.

2. **Pagination**: Always include pagination information for collection endpoints, even when not explicitly requested.

3. **Selective Responses**: Use the $select and $expand parameters to optimize response size and reduce bandwidth usage.

4. **Filtering**: Implement comprehensive filtering support through the FilterOptions model.

5. **Status Codes**: Use appropriate HTTP status codes in ApiResponse to communicate the result of operations.

6. **Metadata**: Use the Metadata dictionary to include additional context information with responses.

## License

GPL-3.0-only