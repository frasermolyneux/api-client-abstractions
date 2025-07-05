# MX.Api.Abstractions

This library provides the core abstractions and common models for standardized API handling in .NET applications. This package serves as the foundation for the MX API Abstractions approach, providing fundamental data structures and interfaces for building API clients and web applications that conform to the API design pattern.

## Installation

```bash
dotnet add package MX.Api.Abstractions
```

## Features

- Standardized API response models following the API design pattern
- Common collection models for result sets with consistent structure
- HTTP response wrappers to separate HTTP concerns from API response models
- Filter options for OData-like query parameters
- Pagination support with comprehensive metadata
- Standardized error models with detailed information

## Core Models

### ApiResponse

The `ApiResponse` class is a wrapper for API responses that don't return data (e.g., DELETE operations, status checks):

```csharp
public class ApiResponse
{
    public ApiError[]? Errors { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

**Use this when:**
- DELETE operations that return no content
- Status check endpoints  
- Validation endpoints that only return success/failure
- Any operation where no data payload is expected

**Note:** The HTTP status code is handled by the `ApiResult` at the transport layer, keeping the API response model focused on business data.

### ApiResponse\<T>

The `ApiResponse<T>` class is a wrapper for API responses that include data:

```csharp
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public ApiError[]? Errors { get; set; }
    public ApiPagination? Pagination { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

**Use this when:**
- GET operations that return resources
- POST operations that return created resources  
- PUT/PATCH operations that return updated resources
- Any operation where a data payload is expected

**Note:** The HTTP status code is handled by the `ApiResult` at the transport layer, keeping the API response model focused on business data.
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

### ApiResult

The `ApiResult` class wraps API responses with HTTP-specific information, providing separation between transport concerns and business data. This class works specifically with non-generic `ApiResponse` objects:

```csharp
public class ApiResult : IApiResult
{
    public ApiResponse? Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    
    // Helper properties
    public bool IsSuccess { get; }
    public bool IsNotFound { get; }
    public bool IsConflict { get; }
}
```

**Use this when:**
- Wrapping API responses that don't return typed data (e.g., DELETE operations, status checks)
- Working with validation endpoints that only return success/failure status
- Handling responses where no specific data payload type is expected

### ApiResult\<T>

The `ApiResult<T>` class wraps strongly-typed API responses with HTTP-specific information, providing separation between transport concerns and business data. This class works specifically with generic `ApiResponse<T>` objects:

```csharp
public class ApiResult<T> : ApiResult, IApiResult<T>
{
    public new ApiResponse<T>? Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    
    // Helper properties  
    public new bool IsSuccess { get; } // Requires both successful status code AND non-null Result
    public bool IsNotFound { get; }
    public bool IsConflict { get; }
}
```

**Use this when:**
- Wrapping API responses that return typed data (e.g., GET, POST, PUT operations)
- Working with endpoints that return specific resource types
- Handling responses where a strongly-typed data payload is expected

**Key differences:**
- `ApiResult` uses `ApiResponse` (non-generic) for responses without typed data
- `ApiResult<T>` uses `ApiResponse<T>` (generic) for responses with typed data
- Generic wrapper's `IsSuccess` property requires both successful HTTP status code AND non-null `Result`
- Non-generic wrapper's `IsSuccess` property only depends on HTTP status code

**Key benefits:**
- Separates HTTP transport concerns from business data models
- Provides status code information at the transport layer
- Offers convenience properties for common status checks
- Enables clean API response models focused on data
- Type-safe handling of different response scenarios
```

## Usage Examples

### Creating API Responses

```csharp
// Non-data response (e.g., DELETE operation)
var deleteResponse = new ApiResponse();

// Non-data error response
var errorResponse = new ApiResponse
{
    Errors = new[]
    {
        new ApiError("INVALID_REQUEST", "The request is invalid")
    }
};

// Success response with data
var successResponse = new ApiResponse<User>
{
    Data = user
};

// Error response with data context
var dataErrorResponse = new ApiResponse<User>
{
    Errors = new[]
    {
        new ApiError("INVALID_USERNAME", "Username is invalid", "username")
    }
};

// Collection response with pagination
var collectionResponse = new ApiResponse<CollectionModel<User>>
{
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

// HTTP Response Wrapper for non-data response (e.g., DELETE operation)
var deleteHttpResponse = new ApiResult
{
    StatusCode = HttpStatusCode.NoContent,
    Result = new ApiResponse()
};

// HTTP Response Wrapper for typed data response  
var httpResponse = new ApiResult<User>
{
    StatusCode = HttpStatusCode.OK,
    Result = successResponse
};

// Alternative constructor usage
var createdResponse = new ApiResult<User>(
    HttpStatusCode.Created, 
    new ApiResponse<User> { Data = newUser }
);

var notFoundResponse = new ApiResult(HttpStatusCode.NotFound);
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
