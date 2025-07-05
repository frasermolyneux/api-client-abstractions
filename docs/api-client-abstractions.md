# MX.Api.Abstractions

A core abstractions library providing common models and interfaces for standardized API handling in .NET applications. This package serves as the foundation for the MX API Abstractions approach, providing fundamental data structures and interfaces for building API clients and web applications that conform to the API design pattern.

## Features

- Standardized API Response Models following the API design pattern
- Collection Models for consistent result sets across all API interactions
- HTTP Response Wrappers to separate HTTP concerns from API response models
- Filter Options for OData-like query parameters with comprehensive support
- Pagination support with detailed metadata for efficient data handling
- Standardized error models with comprehensive error information

## Installation

```bash
dotnet add package MX.Api.Abstractions
```

## Core Components

### API Response Models

The library implements a clear separation between HTTP-level concerns and the API response model. There are two response models depending on whether data is expected:

#### Non-Data Response Model

For operations that don't return data (e.g., DELETE operations, status checks):

```csharp
public class ApiResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public ApiError[]? Errors { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

#### Data Response Model

For operations that return data (e.g., GET, POST with created resource):

```csharp
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
// Creating a non-data response (e.g., for DELETE operation)
ApiResponse deleteResponse = new ApiResponse
{
    StatusCode = HttpStatusCode.NoContent
};

// Creating a non-data error response
ApiResponse errorResponse = new ApiResponse
{
    StatusCode = HttpStatusCode.BadRequest,
    Errors = new[]
    {
        new ApiError("INVALID_REQUEST", "The request is invalid")
    }
};

// Creating a successful data response
ApiResponse<UserDto> successResponse = new ApiResponse<UserDto>
{
    StatusCode = HttpStatusCode.OK,
    Data = new UserDto { Id = "123", Name = "John Doe" }
};

// Creating a data error response
ApiResponse<UserDto> dataErrorResponse = new ApiResponse<UserDto>
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