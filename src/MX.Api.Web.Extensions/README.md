# MX.Api.Web.Extensions

This library provides ASP.NET Core integration as part of the MX API Abstractions approach. It offers extension methods for seamlessly integrating API client responses with ASP.NET Core web applications, simplifying the process of converting API responses to appropriate HTTP responses while maintaining consistency with the API design pattern.

## Installation

```bash
dotnet add package MX.Api.Web.Extensions
```

## Features

- Extension methods to convert ApiResponse<T> objects to IActionResult with proper status codes
- Extension methods to convert ApiResult<T> objects to IActionResult
- Automatic HTTP status code mapping based on API response status
- Preservation of error details and metadata in HTTP responses
- Support for proper pagination headers following API design standards
- Seamless integration between MX.Api.Client and ASP.NET Core applications

## Usage

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