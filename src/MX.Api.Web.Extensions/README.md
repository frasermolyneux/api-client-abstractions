# MX.Api.Web.Extensions

This library provides ASP.NET Core integration as part of the MX API Abstractions approach. It offers extension methods for seamlessly integrating API client responses with ASP.NET Core web applications, simplifying the process of converting API responses to appropriate HTTP responses while maintaining consistency with the API design pattern.

## Installation

```bash
dotnet add package MX.Api.Web.Extensions
```

## Features

- Extension methods to convert ApiResponse<T> objects to IActionResult with proper status codes
- Extension methods to convert ApiResult<T> objects to IActionResult
- Extension methods to convert ApiResponse<T> objects to ApiResult<T> for controller use
- Automatic HTTP status code mapping based on API response status
- Preservation of error details and metadata in HTTP responses
- Support for proper pagination headers following API design standards
- Seamless integration between MX.Api.Client and ASP.NET Core applications

## Usage

### Converting API Responses to API Results

The `ApiResponseExtensions` class provides convenient methods to convert `ApiResponse` and `ApiResponse<T>` objects to `ApiResult` and `ApiResult<T>` objects for use in controllers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserApiClient _apiClient;

    public UsersController(IUserApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var response = await _apiClient.GetUserAsync(id);
        
        // Convert ApiResponse<User> to ApiResult<User> with smart error handling
        var result = response.ToApiResultWithErrorHandling();
        
        // Convert ApiResult to IActionResult
        return result.ToHttpResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var response = await _apiClient.CreateUserAsync(request);
        
        // Convert to Created result (HTTP 201)
        var result = response.ToCreatedResult();
        
        return result.ToHttpResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var response = await _apiClient.UpdateUserAsync(id, request);
        
        // Convert to Accepted result (HTTP 202)
        var result = response.ToAcceptedResult();
        
        return result.ToHttpResult();
    }
}
```

### Available Extension Methods

#### Basic Conversion Methods

- `ToApiResult(HttpStatusCode statusCode = HttpStatusCode.OK)` - Convert with specific status code
- `ToCreatedResult()` - Convert with HTTP 201 Created status
- `ToAcceptedResult()` - Convert with HTTP 202 Accepted status
- `ToNotFoundResult()` - Convert with HTTP 404 Not Found status
- `ToBadRequestResult()` - Convert with HTTP 400 Bad Request status
- `ToConflictResult()` - Convert with HTTP 409 Conflict status

#### Smart Error Handling Methods

- `ToApiResultWithErrorHandling()` - Automatically determines status based on errors and data:
  - Returns HTTP 200 OK if no errors and data is present
  - Returns HTTP 404 Not Found if no errors but data is null (for generic responses)
  - Returns HTTP 400 Bad Request if errors exist

### Basic Usage in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserApiClient _apiClient;

    public UsersController(IUserApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        // Get data from your API client
        var response = await _apiClient.GetUserAsync(id);
        
        // Convert the API response to an appropriate HTTP response
        return response.ToHttpResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] FilterOptions filter)
    {
        // Get collection from your API client
        var response = await _apiClient.GetUsersAsync(filter);
        
        // Convert the API response to an HTTP response with pagination headers
        return response.ToHttpResult(HttpContext);
    }
}
```

### Working with Different Response Types

```csharp
// Converting ApiResponse<T> to IActionResult
public async Task<IActionResult> GetResource(string id)
{
    ApiResponse<ResourceDto> response = await _apiClient.GetResourceAsync(id);
    return response.ToHttpResult();
}

// Converting ApiResult<T> to IActionResult
public async Task<IActionResult> GetResource(string id)
{
    ApiResult<ResourceDto> wrapper = await _apiClient.GetResourceWithWrapperAsync(id);
    return wrapper.ToHttpResult();
}
```

### Adding Pagination Headers

When returning collections, you can add pagination headers to the response:

```csharp
public async Task<IActionResult> GetResources([FromQuery] FilterOptions filter)
{
    var response = await _apiClient.GetResourcesAsync(filter);
    
    // Adds pagination headers to the HTTP response
    return response.ToHttpResult(HttpContext);
}
```

### Error Handling

The extension methods automatically handle error responses:

```csharp
public async Task<IActionResult> CreateResource([FromBody] ResourceCreateDto dto)
{
    var response = await _apiClient.CreateResourceAsync(dto);
    
    // Will return appropriate status code and error details if creation fails
    return response.ToHttpResult();
}
```

## Advanced Usage

### Custom Response Formatting

You can customize how responses are formatted:

```csharp
public async Task<IActionResult> GetCustomFormattedResponse(string id)
{
    var response = await _apiClient.GetResourceAsync(id);
    
    return response.ToHttpResult(formatResponse: result => 
    {
        // Custom response formatting
        return new
        {
            resource = result,
            timestamp = DateTime.UtcNow,
            version = "1.0"
        };
    });
}
```

## License

GPL-3.0-only