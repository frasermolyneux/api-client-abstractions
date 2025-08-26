# MX.Api.Web.Extensions

ASP.NET Core integration library providing extension methods for converting API response objects to HTTP results. Seamlessly bridges API clients and web applications following the API design pattern.

## Installation

```bash
dotnet add package MX.Api.Web.Extensions
```

## Key Features

- **🔄 Response Conversion** - Convert `ApiResponse<T>` to `IActionResult` with proper status codes
- **📊 Smart Status Mapping** - Automatic HTTP status code assignment based on response content
- **🚀 Controller Simplification** - Reduce boilerplate code in API controllers
- **🔗 Client Integration** - Seamless integration between API clients and web applications
- **📋 Error Preservation** - Maintain error details and metadata in HTTP responses

## Quick Start

### In API Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        
        if (user == null)
        {
            var response = new ApiResponse<User>(
                new ApiError("USER_NOT_FOUND", $"User {id} not found"));
            return response.ToApiResult(HttpStatusCode.NotFound).ToHttpResult();
        }
        
        var successResponse = new ApiResponse<User>(user);
        return successResponse.ToApiResult().ToHttpResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        var response = new ApiResponse<User>(user);
        return response.ToApiResult(HttpStatusCode.Created).ToHttpResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        var response = new ApiResponse(); // Success with no data
        return response.ToApiResult(HttpStatusCode.NoContent).ToHttpResult();
    }
}
```

### With API Client Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProxyController : ControllerBase
{
    private readonly IExternalApiClient _apiClient;

    public ProxyController(IExternalApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserFromExternalApi(int id)
    {
        // API client returns ApiResult<User>
        var apiResult = await _apiClient.GetUserAsync(id);
        
        // Convert directly to HTTP result
        return apiResult.ToHttpResult();
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var apiResult = await _apiClient.GetProductsAsync(page, pageSize);
        return apiResult.ToHttpResult();
    }
}
```

## Extension Methods

### ApiResponse to ApiResult

Convert API response models to HTTP-aware results:

```csharp
// Success response (200 OK)
var response = new ApiResponse<Product>(product);
var result = response.ToApiResult();

// Success with custom status code (201 Created)
var result = response.ToApiResult(HttpStatusCode.Created);

// Error response (400 Bad Request)
var errorResponse = new ApiResponse<Product>(
    new ApiError("VALIDATION_ERROR", "Invalid product data"));
var result = errorResponse.ToApiResult(HttpStatusCode.BadRequest);
```

### ApiResult to IActionResult

Convert API results to ASP.NET Core action results:

```csharp
public async Task<IActionResult> GetProduct(int id)
{
    var apiResult = await _productService.GetProductAsync(id);
    
    // Automatically maps status codes to appropriate IActionResult types
    return apiResult.ToHttpResult();
    
    // Results in:
    // - 200 OK -> Ok(response)
    // - 404 Not Found -> NotFound(response)  
    // - 400 Bad Request -> BadRequest(response)
    // - 500 Internal Server Error -> StatusCode(500, response)
}
```

### Error Handling with Smart Mapping

```csharp
public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
{
    try
    {
        var product = await _productService.UpdateProductAsync(id, request);
        var response = new ApiResponse<Product>(product);
        return response.ToApiResult().ToHttpResult();
    }
    catch (ValidationException ex)
    {
        var errorResponse = new ApiResponse<Product>(
            new ApiError("VALIDATION_ERROR", ex.Message));
        return errorResponse.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
    }
    catch (NotFoundException ex)
    {
        var errorResponse = new ApiResponse<Product>(
            new ApiError("PRODUCT_NOT_FOUND", ex.Message));
        return errorResponse.ToApiResult(HttpStatusCode.NotFound).ToHttpResult();
    }
}
```

## Advanced Scenarios

### Collection Responses with Pagination

```csharp
[HttpGet]
public async Task<IActionResult> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? category = null)
{
    var products = await _productService.GetProductsAsync(page, pageSize, category);
    
    var collection = new CollectionModel<Product>
    {
        Items = products.Items
    };

    var response = new ApiResponse<CollectionModel<Product>>(collection)
    {
        Pagination = new ApiPagination
        {
            TotalCount = products.TotalCount,
            FilteredCount = products.FilteredCount,
            Skip = (page - 1) * pageSize,
            Top = pageSize,
            HasMore = products.FilteredCount > page * pageSize
        }
    };
    return response.ToApiResult().ToHttpResult();
}
```

### Custom Status Code Mapping

```csharp
public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
{
    var result = await _paymentService.ProcessPaymentAsync(request);
    
    return result.Status switch
    {
        PaymentStatus.Success => new ApiResponse<PaymentResult>(result)
            .ToApiResult(HttpStatusCode.OK).ToHttpResult(),
            
        PaymentStatus.Pending => new ApiResponse<PaymentResult>(result)
            .ToApiResult(HttpStatusCode.Accepted).ToHttpResult(),
            
        PaymentStatus.InsufficientFunds => new ApiResponse<PaymentResult>(
            new ApiError("INSUFFICIENT_FUNDS", "Payment declined due to insufficient funds"))
            .ToApiResult(HttpStatusCode.PaymentRequired).ToHttpResult(),
            
        PaymentStatus.Declined => new ApiResponse<PaymentResult>(
            new ApiError("PAYMENT_DECLINED", "Payment was declined"))
            .ToApiResult(HttpStatusCode.BadRequest).ToHttpResult(),
            
        _ => new ApiResponse<PaymentResult>(
            new ApiError("PAYMENT_ERROR", "An error occurred processing the payment"))
            .ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult()
    };
}
```

### Metadata and Headers

```csharp
public async Task<IActionResult> GetUserWithMetadata(int id)
{
    var user = await _userService.GetUserAsync(id);
    
    var response = new ApiResponse<User>(user)
    {
        Metadata = new Dictionary<string, string>
        {
            ["Cache-Control"] = "max-age=300",
            ["Last-Modified"] = DateTime.UtcNow.ToString("R"),
            ["X-User-Role"] = user.Role
        }
    };

    var result = response.ToApiResult();
    
    // Add metadata as response headers
    if (result.Result?.Metadata != null)
    {
        foreach (var (key, value) in result.Result.Metadata)
        {
            Response.Headers.Add(key, value);
        }
    }

    return result.ToHttpResult();
}
```

## Integration Patterns

### API Gateway/Proxy Pattern

```csharp
[ApiController]
[Route("api/gateway/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly IUserApiClient _userClient;
    private readonly IOrderApiClient _orderClient;

    public GatewayController(IUserApiClient userClient, IOrderApiClient orderClient)
    {
        _userClient = userClient;
        _orderClient = orderClient;
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userClient.GetUserAsync(id);
        return result.ToHttpResult(); // Direct passthrough
    }

    [HttpGet("users/{userId}/orders")]
    public async Task<IActionResult> GetUserOrders(int userId)
    {
        // Aggregate multiple API calls
        var userResult = await _userClient.GetUserAsync(userId);
        if (!userResult.IsSuccess)
            return userResult.ToHttpResult();

        var ordersResult = await _orderClient.GetOrdersByUserAsync(userId);
        return ordersResult.ToHttpResult();
    }
}
```

### Backend for Frontend (BFF) Pattern

```csharp
[ApiController]
[Route("api/bff/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IUserApiClient _userClient;
    private readonly IOrderApiClient _orderClient;
    private readonly IAnalyticsApiClient _analyticsClient;

    [HttpGet("summary/{userId}")]
    public async Task<IActionResult> GetDashboardSummary(int userId)
    {
        // Aggregate data from multiple APIs
        var userTask = _userClient.GetUserAsync(userId);
        var ordersTask = _orderClient.GetRecentOrdersAsync(userId, 5);
        var analyticsTask = _analyticsClient.GetUserAnalyticsAsync(userId);

        await Task.WhenAll(userTask, ordersTask, analyticsTask);

        // Compose custom response
        var summary = new DashboardSummary
        {
            User = userTask.Result.IsSuccess ? userTask.Result.Result?.Data : null,
            RecentOrders = ordersTask.Result.IsSuccess ? ordersTask.Result.Result?.Data : null,
            Analytics = analyticsTask.Result.IsSuccess ? analyticsTask.Result.Result?.Data : null
        };

        var response = new ApiResponse<DashboardSummary>(summary);
        return response.ToApiResult().ToHttpResult();
    }
}
```

## Status Code Mappings

The extension methods automatically map common scenarios:

| Condition                    | HTTP Status Code | IActionResult Type |
| ---------------------------- | ---------------- | ------------------ |
| `ApiResponse.Errors == null` | 200 OK           | `Ok()`             |
| Custom status provided       | As specified     | `StatusCode()`     |
| `ApiResult.IsNotFound`       | 404 Not Found    | `NotFound()`       |
| `ApiResult.IsBadRequest`     | 400 Bad Request  | `BadRequest()`     |
| `ApiResult.IsUnauthorized`   | 401 Unauthorized | `Unauthorized()`   |
| Other error status codes     | As specified     | `StatusCode()`     |

## Dependencies

This package depends on:
- **MX.Api.Abstractions** - Core response models and interfaces
- **Microsoft.AspNetCore.Mvc.Core** - ASP.NET Core MVC abstractions
- **Microsoft.Extensions.Logging** - Logging abstractions

## Documentation

- **[📖 Implementation Guide - API Providers](../../docs/implementing-api-provider.md)** - Building APIs with these extensions
- **[📖 API Design Patterns](../../docs/api-design-v2.md)** - Understanding the design principles

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