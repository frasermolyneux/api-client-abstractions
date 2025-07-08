# Implementation Guide: Building APIs with MX API Abstractions

This guide shows how to implement APIs that follow the MX API Abstractions pattern, creating a consistent and standards-compliant API that can be easily consumed by clients using this library.

## Table of Contents
- [Overview](#overview)
- [Project Setup](#project-setup)
- [Controller Implementation](#controller-implementation)
- [Response Patterns](#response-patterns)
- [Error Handling](#error-handling)
- [Creating Client Libraries](#creating-client-libraries)
- [Best Practices](#best-practices)

## Overview

When building an API using the MX API Abstractions pattern, you'll create:

1. **API Controllers** that return standardized `ApiResponse<T>` objects
2. **Client Library** that provides strongly-typed access to your API
3. **Response Models** that follow consistent patterns
4. **Error Handling** that uses standardized error codes and messages

## Project Setup

### 1. Create Your API Project

```bash
dotnet new webapi -n MyCompany.ProductCatalog.Api
cd MyCompany.ProductCatalog.Api
dotnet add package MX.Api.Abstractions
dotnet add package MX.Api.Web.Extensions
```

### 2. Project Structure

```
MyCompany.ProductCatalog.Api/
├── Controllers/
│   ├── ProductsController.cs
│   └── CategoriesController.cs
├── Models/
│   ├── Product.cs
│   ├── Category.cs
│   ├── CreateProductRequest.cs
│   └── UpdateProductRequest.cs
├── Services/
│   └── IProductService.cs
└── Program.cs
```

### 3. Configure Services

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add your business services
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Controller Implementation

### Basic Controller Pattern

```csharp
using Microsoft.AspNetCore.Mvc;
using MX.Api.Abstractions;
using MX.Api.Web.Extensions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductService _productService;

    public ProductsController(ILogger<ProductsController> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    /// <summary>
    /// Get all products with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] bool? inStock = null)
    {
        try
        {
            _logger.LogInformation("Getting products page {Page} with size {PageSize}", page, pageSize);

            var products = await _productService.GetProductsAsync(page, pageSize, category, inStock);
            
            var collection = new CollectionModel<Product>
            {
                Items = products.Items,
                TotalCount = products.TotalCount,
                FilteredCount = products.FilteredCount,
                Pagination = new ApiPagination
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)products.TotalCount / pageSize)
                }
            };

            var response = new ApiResponse<CollectionModel<Product>>(collection);
            return response.ToApiResult().ToHttpResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            var errorResponse = new ApiResponse<CollectionModel<Product>>(
                new ApiError("INTERNAL_ERROR", "An error occurred while retrieving products"));
            return errorResponse.ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult();
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("Getting product {ProductId}", id);

            var product = await _productService.GetProductAsync(id);
            
            if (product == null)
            {
                var notFoundResponse = new ApiResponse<Product>(
                    new ApiError("PRODUCT_NOT_FOUND", $"Product with ID {id} was not found"));
                return notFoundResponse.ToApiResult(HttpStatusCode.NotFound).ToHttpResult();
            }

            var response = new ApiResponse<Product>(product);
            return response.ToApiResult().ToHttpResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            var errorResponse = new ApiResponse<Product>(
                new ApiError("INTERNAL_ERROR", "An error occurred while retrieving the product"));
            return errorResponse.ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult();
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            _logger.LogInformation("Creating product {ProductName}", request.Name);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                var validationResponse = new ApiResponse<Product>(
                    new ApiError("VALIDATION_ERROR", "Product name is required"));
                return validationResponse.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
            }

            var product = await _productService.CreateProductAsync(request);
            
            var response = new ApiResponse<Product>(product);
            return response.ToApiResult(HttpStatusCode.Created).ToHttpResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            var errorResponse = new ApiResponse<Product>(
                new ApiError("INTERNAL_ERROR", "An error occurred while creating the product"));
            return errorResponse.ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult();
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            _logger.LogInformation("Updating product {ProductId}", id);

            var existingProduct = await _productService.GetProductAsync(id);
            if (existingProduct == null)
            {
                var notFoundResponse = new ApiResponse<Product>(
                    new ApiError("PRODUCT_NOT_FOUND", $"Product with ID {id} was not found"));
                return notFoundResponse.ToApiResult(HttpStatusCode.NotFound).ToHttpResult();
            }

            var updatedProduct = await _productService.UpdateProductAsync(id, request);
            
            var response = new ApiResponse<Product>(updatedProduct);
            return response.ToApiResult().ToHttpResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            var errorResponse = new ApiResponse<Product>(
                new ApiError("INTERNAL_ERROR", "An error occurred while updating the product"));
            return errorResponse.ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult();
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product {ProductId}", id);

            var existingProduct = await _productService.GetProductAsync(id);
            if (existingProduct == null)
            {
                var notFoundResponse = new ApiResponse(
                    new ApiError("PRODUCT_NOT_FOUND", $"Product with ID {id} was not found"));
                return notFoundResponse.ToApiResult(HttpStatusCode.NotFound).ToHttpResult();
            }

            await _productService.DeleteProductAsync(id);
            
            var response = new ApiResponse(); // Success with no data
            return response.ToApiResult(HttpStatusCode.NoContent).ToHttpResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            var errorResponse = new ApiResponse(
                new ApiError("INTERNAL_ERROR", "An error occurred while deleting the product"));
            return errorResponse.ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult();
        }
    }
}
```

## Response Patterns

### 1. Success Responses

```csharp
// Single item
var response = new ApiResponse<Product>(product);
return response.ToApiResult().ToHttpResult();

// Collection with pagination
var collection = new CollectionModel<Product>
{
    Items = products,
    TotalCount = totalCount,
    FilteredCount = filteredCount,
    Pagination = new ApiPagination { Page = 1, PageSize = 10, TotalPages = 5 }
};
var response = new ApiResponse<CollectionModel<Product>>(collection);
return response.ToApiResult().ToHttpResult();

// No content (e.g., DELETE operations)
var response = new ApiResponse();
return response.ToApiResult(HttpStatusCode.NoContent).ToHttpResult();
```

### 2. Error Responses

```csharp
// Not found
var response = new ApiResponse<Product>(
    new ApiError("PRODUCT_NOT_FOUND", "Product was not found"));
return response.ToApiResult(HttpStatusCode.NotFound).ToHttpResult();

// Validation error
var response = new ApiResponse<Product>(
    new ApiError("VALIDATION_ERROR", "Product name is required"));
return response.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();

// Internal error
var response = new ApiResponse<Product>(
    new ApiError("INTERNAL_ERROR", "An unexpected error occurred"));
return response.ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult();
```

## Error Handling

### 1. Standardized Error Codes

Create a constants class for consistent error codes:

```csharp
public static class ApiErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string InternalError = "INTERNAL_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    
    // Domain-specific error codes
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string DuplicateProduct = "DUPLICATE_PRODUCT";
}
```

### 2. Global Exception Handler

```csharp
// GlobalExceptionMiddleware.cs
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ApiResponse(
            new ApiError("INTERNAL_ERROR", "An unexpected error occurred"));
        
        var apiResult = response.ToApiResult(HttpStatusCode.InternalServerError);
        
        context.Response.StatusCode = (int)apiResult.StatusCode;
        context.Response.ContentType = "application/json";
        
        var json = JsonConvert.SerializeObject(apiResult.Result);
        await context.Response.WriteAsync(json);
    }
}

// Register in Program.cs
app.UseMiddleware<GlobalExceptionMiddleware>();
```

## Creating Client Libraries

### 1. Create Client Project

```bash
dotnet new classlib -n MyCompany.ProductCatalog.Client
cd MyCompany.ProductCatalog.Client
dotnet add package MX.Api.Client
```

### 2. Define Client Interface

```csharp
// IProductCatalogClient.cs
public interface IProductCatalogClient
{
    Task<ApiResult<CollectionModel<Product>>> GetProductsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? category = null, 
        bool? inStock = null, 
        CancellationToken cancellationToken = default);
        
    Task<ApiResult<Product>> GetProductAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResult<Product>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult<Product>> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult<Product>> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
```

### 3. Implement Client

```csharp
// ProductCatalogClient.cs
public class ProductCatalogClient : BaseApi, IProductCatalogClient
{
    private readonly ILogger<ProductCatalogClient> _logger;

    public ProductCatalogClient(
        ILogger<ProductCatalogClient> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        IOptions<ApiClientOptions> options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
        _logger = logger;
    }

    public async Task<ApiResult<CollectionModel<Product>>> GetProductsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? category = null, 
        bool? inStock = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting products page {Page} with size {PageSize}", page, pageSize);

            var request = await CreateRequestAsync("products", Method.Get, cancellationToken);
            
            request.AddQueryParameter("page", page.ToString());
            request.AddQueryParameter("pageSize", pageSize.ToString());
            
            if (!string.IsNullOrWhiteSpace(category))
                request.AddQueryParameter("category", category);
                
            if (inStock.HasValue)
                request.AddQueryParameter("inStock", inStock.Value.ToString());

            var response = await ExecuteAsync(request, false, cancellationToken);
            return response.ToApiResponse<CollectionModel<Product>>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to get products");
            var errorResponse = new ApiResponse<CollectionModel<Product>>(
                new ApiError("CLIENT_ERROR", "Failed to retrieve products"));
            return new ApiResult<CollectionModel<Product>>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<ApiResult<Product>> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting product {ProductId}", id);

            var request = await CreateRequestAsync($"products/{id}", Method.Get, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            
            return response.ToApiResponse<Product>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to get product {ProductId}", id);
            var errorResponse = new ApiResponse<Product>(
                new ApiError("CLIENT_ERROR", "Failed to retrieve product"));
            return new ApiResult<Product>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<ApiResult<Product>> CreateProductAsync(
        CreateProductRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating product {ProductName}", request.Name);

            var restRequest = await CreateRequestAsync("products", Method.Post, cancellationToken);
            restRequest.AddJsonBody(request);
            
            var response = await ExecuteAsync(restRequest, false, cancellationToken);
            return response.ToApiResponse<Product>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create product");
            var errorResponse = new ApiResponse<Product>(
                new ApiError("CLIENT_ERROR", "Failed to create product"));
            return new ApiResult<Product>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<ApiResult<Product>> UpdateProductAsync(
        int id, 
        UpdateProductRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating product {ProductId}", id);

            var restRequest = await CreateRequestAsync($"products/{id}", Method.Put, cancellationToken);
            restRequest.AddJsonBody(request);
            
            var response = await ExecuteAsync(restRequest, false, cancellationToken);
            return response.ToApiResponse<Product>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update product {ProductId}", id);
            var errorResponse = new ApiResponse<Product>(
                new ApiError("CLIENT_ERROR", "Failed to update product"));
            return new ApiResult<Product>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<ApiResult<Product>> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting product {ProductId}", id);

            var request = await CreateRequestAsync($"products/{id}", Method.Delete, cancellationToken);
            var response = await ExecuteAsync(request, false, cancellationToken);
            
            return response.ToApiResponse<Product>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}", id);
            var errorResponse = new ApiResponse<Product>(
                new ApiError("CLIENT_ERROR", "Failed to delete product"));
            return new ApiResult<Product>(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
```

### 4. Client Registration Extension

```csharp
// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductCatalogClient(
        this IServiceCollection services, 
        Action<ApiClientOptions> configureOptions)
    {
        services.AddApiClient();
        services.Configure(configureOptions);
        services.AddTransient<IProductCatalogClient, ProductCatalogClient>();
        
        return services;
    }
}
```

## Best Practices

### 1. Consistent Naming Conventions

- Use consistent error codes across your API
- Follow RESTful URL patterns (`/products/{id}`)
- Use standard HTTP status codes appropriately

### 2. Logging and Monitoring

```csharp
public async Task<IActionResult> GetProduct(int id)
{
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["ProductId"] = id,
        ["Operation"] = "GetProduct"
    });

    _logger.LogInformation("Getting product {ProductId}", id);
    
    // Implementation...
    
    _logger.LogInformation("Successfully retrieved product {ProductId}", id);
}
```

### 3. Validation

```csharp
// Use data annotations on models
public class CreateProductRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

// Validate in controllers
[HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
{
    if (!ModelState.IsValid)
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToArray();
            
        var response = new ApiResponse<Product>(
            new ApiError("VALIDATION_ERROR", string.Join("; ", errors)));
        return response.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
    }
    
    // Continue with creation...
}
```

### 4. Testing

```csharp
// Integration test example
[Test]
public async Task GetProduct_WhenProductExists_ReturnsProduct()
{
    // Arrange
    var client = _factory.CreateClient();
    var productId = 1;

    // Act
    var response = await client.GetAsync($"/api/products/{productId}");
    var content = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Product>>(content);

    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    Assert.IsNotNull(apiResponse.Data);
    Assert.AreEqual(productId, apiResponse.Data.Id);
}
```

### 5. Documentation

- Use XML comments on controllers and methods
- Provide Swagger/OpenAPI documentation
- Include example requests and responses
- Document error codes and their meanings

### 6. Versioning

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[ApiVersion("1.0")]
public class ProductsV1Controller : ControllerBase
{
    // V1 implementation
}

[ApiController]
[Route("api/v2/[controller]")]
[ApiVersion("2.0")]
public class ProductsV2Controller : ControllerBase
{
    // V2 implementation
}
```

This comprehensive guide should help you implement APIs that fully leverage the MX API Abstractions pattern, ensuring consistency, maintainability, and ease of consumption by client applications.
