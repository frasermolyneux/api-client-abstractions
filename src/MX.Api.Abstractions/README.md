# MX.Api.Abstractions

This library provides common models and interfaces for standardized API response handling, including pagination, filtering, and error management.

## Installation

```bash
dotnet add package MX.Api.Abstractions
```

## Features

- Standardized API response models
- Common collection models for result sets
- Consistent error model structure
- Pagination support with metadata
- OData-like filtering options

## Core Models

### ApiResponse\<T>

The `ApiResponse<T>` class is a wrapper for API responses that includes:

```csharp
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
}
```

### ApiError

The `ApiError` class provides a standardized format for API errors:

```csharp
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Target { get; set; }
    public ApiError[]? Details { get; set; }
}
```

### ApiPagination

The `ApiPagination` class provides standardized pagination information:

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

### CollectionModel\<T>

The `CollectionModel<T>` class provides a standardized container for collections of resources:

```csharp
public class CollectionModel<T>
{
    public List<T> Items { get; set; } = new List<T>();
}
```

### FilterOptions

The `FilterOptions` class provides standardized options for filtering API responses:

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

### HttpResponseWrapper\<T>

The `HttpResponseWrapper<T>` class wraps API responses with HTTP-specific information:

```csharp
public class HttpResponseWrapper<T>
{
    public ApiResponse<T>? Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? Content { get; set; }
    
    // Helper properties
    public bool IsSuccess { get; }
    public bool IsNotFound { get; }
    public bool IsConflict { get; }
    public bool IsBadRequest { get; }
}
```

## Usage Examples

### Creating API Responses

```csharp
// Success response with data
var successResponse = new ApiResponse<User>
{
    StatusCode = HttpStatusCode.OK,
    Data = user
};

// Error response
var errorResponse = new ApiResponse<User>
{
    StatusCode = HttpStatusCode.BadRequest,
    Errors = new[]
    {
        new ApiError
        {
            Code = "InvalidUsername",
            Message = "Username is invalid",
            Target = "username"
        }
    }
};

// Collection response with pagination
var collectionResponse = new ApiResponse<CollectionModel<User>>
{
    StatusCode = HttpStatusCode.OK,
    Data = new CollectionModel<User>
    {
        Items = users
    },
    Pagination = new ApiPagination
    {
        TotalCount = 100,
        FilteredCount = 10,
        Skip = 0,
        Top = 10,
        HasMore = true
    }
};
```

### Working with Filter Options

```csharp
// Create filter options for an API request
var filter = new FilterOptions
{
    FilterExpression = "username eq 'john' and active eq true",
    OrderBy = "created desc",
    Skip = 0,
    Top = 20,
    Select = new[] { "username", "email", "firstName", "lastName" },
    Expand = new[] { "roles", "permissions" }
};
```
