# API Design Pattern

## Overview

The API design pattern provides a consistent approach to building RESTful APIs with standardized URL structure, query parameters, response formats, and error handling. This design pattern addresses common issues with API design, such as inconsistent filtering, lack of count-only queries, inefficient relationship handling, and lack of standardized query patterns.

## Key Principles

1. **Consistent URL Structure** - Clear and consistent resource naming and URL structure
2. **Uniform Parameter Handling** - Standardized query parameters for filtering, sorting, and pagination
3. **Standardized Response Format** - Unified response structure for all endpoints
4. **Comprehensive Error Model** - Detailed error information with consistent structure
5. **Efficient Relationship Handling** - Use of entity expansion instead of separate requests

## URL Structure

The API follows a RESTful URL structure:

```
/{resource}/{id}
/{resource}/{id}/{subresource}
/{resource}/{id}/{subresource}/{subresourceId}
```

Examples:
- `/users` - Get all users
- `/users/123` - Get user with ID 123
- `/users/123/permissions` - Get permissions for user with ID 123
- `/users/123/permissions/456` - Get specific permission with ID 456 for user with ID 123

## Common Query Parameters

All collection endpoints support the following standardized query parameters:

| Parameter  | Description                                           | Example                                |
| ---------- | ----------------------------------------------------- | -------------------------------------- |
| `$filter`  | OData-like filter expression                          | `$filter=status eq 'active'`           |
| `$select`  | Select specific fields                                | `$select=name,email,phone`             |
| `$expand`  | Expand related entities                               | `$expand=profile,roles`                |
| `$orderby` | Sort by field(s)                                      | `$orderby=lastName asc,firstName desc` |
| `$top`     | Number of records to take                             | `$top=10`                              |
| `$skip`    | Number of records to skip                             | `$skip=20`                             |
| `$count`   | When true, returns only the count of matching records | `$count=true`                          |

## Response Models

### Standard Response Models

All API endpoints use consistent response formats. There are two response model types:

#### ApiResponse (Non-Data Responses)

For operations that don't return data (e.g., DELETE operations, status checks):

```csharp
public class ApiResponse
{
    public ApiError[]? Errors { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

**Note:** HTTP status codes are handled by the HTTP transport layer (ApiResult), keeping the API response model focused on business data.

#### ApiResponse\<T> (Data Responses)

For operations that return data (e.g., GET, POST with created resource):

```csharp
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public ApiError[]? Errors { get; set; }
    public ApiPagination? Pagination { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

**Note:** HTTP status codes are handled by the HTTP transport layer (ApiResult), keeping the API response model focused on business data.

#### Supporting Models

```csharp
public class ApiError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string? Target { get; set; }
    public ApiError[]? Details { get; set; }
}

public class ApiPagination
{
    public int TotalCount { get; set; }
    public int FilteredCount { get; set; }
    public int Skip { get; set; }
    public int Top { get; set; }
    public bool HasMore { get; set; }
}
```

### Collection Response Format

Collections use a standard wrapper:

```csharp
public class CollectionModel<T>
{
    public List<T> Items { get; set; } = new List<T>();
}
```

## Filter Expressions

The API uses OData-like filter expressions for querying data. Examples of filter expressions:

- Simple equality: `status eq 'active'`
- Multiple conditions: `status eq 'active' and createdDate gt 2023-01-01`
- Logical operators: `(status eq 'active' or status eq 'pending') and not deleted`
- String functions: `startswith(name, 'J') eq true`
- Collection functions: `tags/any(t: t eq 'important')`

## Entity Expansion

Rather than using flags or separate requests for related data, the API uses a flexible expansion model:

```
$expand=aliases,ipAddresses,adminActions
```

This allows for more granular control over which related entities are included in the response.

## Example Requests

### Get Collection with Filtering and Pagination

```
GET /players?$filter=gameType eq 'CallOfDuty2' and lastSeen gt '2023-01-01'&$top=10&$skip=0&$orderby=username asc
```

Response (HTTP 200):

```json
{
  "data": {
    "items": [
      { "playerId": 123, "username": "Player1", "gameType": "CallOfDuty2", "lastSeen": "2023-05-10" },
      { "playerId": 456, "username": "Player2", "gameType": "CallOfDuty2", "lastSeen": "2023-04-15" }
    ]
  },
  "pagination": {
    "totalCount": 150,
    "filteredCount": 42,
    "skip": 0,
    "top": 10,
    "hasMore": true
  },
  "metadata": {
    "requestId": "abc-123-xyz"
  }
}
```

### Get Single Entity with Expanded Relations

```
GET /players/1234?$expand=aliases,adminActions,ipAddresses
```

Response (HTTP 200):

```json
{
  "data": {
    "playerId": 1234,
    "username": "MainPlayer",
    "gameType": "CallOfDuty2",
    "lastSeen": "2023-05-20",
    "aliases": [
      { "aliasId": 1, "name": "OtherName1" },
      { "aliasId": 2, "name": "OtherName2" }
    ],
    "adminActions": [
      { "actionId": 101, "actionType": "Ban", "date": "2023-01-15" }
    ],
    "ipAddresses": [
      { "ipAddressId": 500, "address": "192.168.1.1", "lastSeen": "2023-05-20" }
    ]
  },
  "metadata": {
    "requestId": "def-456-uvw"
  }
}
```

### Count-Only Query

```
GET /players?$filter=gameType eq 'CallOfDuty4' and username startswith 'John'&$count=true
```

Response (HTTP 200):

```json
{
  "data": 27,
  "metadata": {
    "requestId": "ghi-789-rst"
  }
}
```

### Error Response

```
POST /players
```

Response (HTTP 400):

```json
{
  "errors": [
    {
      "code": "ValidationError",
      "message": "The request is invalid",
      "details": [
        {
          "code": "RequiredField",
          "message": "Username is required",
          "target": "username"
        },
        {
          "code": "InvalidFormat",
          "message": "Email format is invalid",
          "target": "email"
        }
      ]
    }
  ],
  "metadata": {
    "requestId": "jkl-012-opq"
  }
}
```

## Implementation Guidelines

### API Client Implementation

For implementing API clients that work with the API design:

1. **Use Standardized Models**:
   - Use the `ApiResponse` class for operations without data (e.g., DELETE operations)
   - Use the `ApiResponse<T>` class for operations with data
   - Use `ApiError` and `ApiPagination` classes for consistent error handling and pagination
   - Use `CollectionModel<T>` for collection endpoints
   - Use `FilterOptions` for query parameters

2. **Request Building**:
   - Add query parameters with standard parameter names
   - Use extension methods for adding filter options
   - Include Accept and Content-Type headers

3. **Response Handling**:
   - Deserialize responses into appropriate models
   - Check status codes for success/failure
   - Handle errors consistently
   - Process pagination information

4. **Error Handling**:
   - Handle transient errors with retry policies
   - Process API errors from the error model
   - Provide detailed error information

### API Implementation

For implementing APIs that follow the API design:

1. **Request Processing**:
   - Validate query parameters
   - Parse filter expressions
   - Handle pagination parameters
   - Process entity expansion requests

2. **Response Building**:
   - Use standard response models
   - Include pagination information
   - Add appropriate metadata
   - Use standard error format
   - Set correct HTTP status codes

3. **Entity Projection and Expansion**:
   - Implement efficient data loading
   - Use select expressions to limit fields
   - Use expand expressions to include relations
   - Optimize database queries

4. **Security and Performance**:
   - Implement proper authentication
   - Add rate limiting
   - Use appropriate caching
   - Add logging and telemetry

## Best Practices

1. **Filter Parser**: Implement a flexible filter parser that can translate OData-like filter expressions into LINQ or SQL queries.

2. **Entity Projection**: Use the `$select` parameter to project only the requested fields, reducing response size and improving performance.

3. **Entity Expansion**: Use the `$expand` parameter to include related entities in the response, avoiding multiple API calls.

4. **Count-Only Queries**: Optimize count-only queries by not retrieving the actual data.

5. **Bulk Operations**: Implement bulk operations for create, update, and delete to reduce the number of API calls.

6. **Metadata**: Include metadata in the response such as entity counts, filtering constraints, etc.

7. **Consistent Error Handling**: Use standard error codes and formats across all endpoints.

8. **Version Header**: Include API version information in response headers.

9. **Caching**: Implement appropriate caching headers for improved performance.

10. **Rate Limiting**: Consider implementing rate limiting to protect the API from abuse.

11. **Documentation**: Provide comprehensive API documentation with examples for common scenarios.

12. **Testing**: Create unit and integration tests for all endpoints and scenarios.