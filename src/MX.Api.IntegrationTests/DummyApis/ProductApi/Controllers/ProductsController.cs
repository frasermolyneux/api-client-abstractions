using Microsoft.AspNetCore.Mvc;
using MX.Api.Abstractions;
using MX.Api.IntegrationTests.Constants;
using MX.Api.IntegrationTests.DummyApis.ProductApi.Models;
using MX.Api.IntegrationTests.Clients.ProductApiClient.Interfaces.V1;
using MX.Api.Web.Extensions;

namespace MX.Api.IntegrationTests.DummyApis.ProductApi.Controllers;

/// <summary>
/// Product API controller for testing
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase, IProductApiV1
{
    private static readonly List<Product> Products = new()
    {
        new() { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m, InStock = true },
        new() { Id = 2, Name = "Desk Chair", Category = "Furniture", Price = 199.99m, InStock = true },
        new() { Id = 3, Name = "Coffee Mug", Category = "Kitchen", Price = 9.99m, InStock = false },
        new() { Id = 4, Name = "Notebook", Category = "Stationery", Price = 4.99m, InStock = true },
        new() { Id = 5, Name = "Monitor", Category = "Electronics", Price = 299.99m, InStock = true }
    };

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>Collection of products</returns>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var response = await ((IProductApiV1)this).GetProductsAsync();
        return response.ToHttpResult();
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product or not found</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var response = await ((IProductApiV1)this).GetProductAsync(id);
        return response.ToHttpResult();
    }

    // Interface implementation methods

    /// <summary>
    /// Get all products implementation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products</returns>
    async Task<ApiResult<CollectionModel<Product>>> IProductApiV1.GetProductsAsync(CancellationToken cancellationToken)
    {
        // Check for authentication header
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer test-product-key"))
        {
            return new ApiResponse<CollectionModel<Product>>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.BearerTokenRequired)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized);
        }

        var collection = new CollectionModel<Product>
        {
            Items = Products,
            TotalCount = Products.Count,
            FilteredCount = Products.Count
        };

        return new ApiResponse<CollectionModel<Product>>(collection).ToApiResult();
    }

    /// <summary>
    /// Get product by ID implementation
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product or error</returns>
    async Task<ApiResult<Product>> IProductApiV1.GetProductAsync(int id, CancellationToken cancellationToken)
    {
        // Check for authentication header
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer test-product-key"))
        {
            return new ApiResponse<Product>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.InvalidOrMissingAuthentication,
                    ApiErrorConstants.ErrorDetails.BearerTokenRequired)
            ).ToApiResult(System.Net.HttpStatusCode.Unauthorized);
        }

        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return new ApiResponse<Product>(
                new ApiError(ApiErrorConstants.ErrorCodes.NotFound,
                    string.Format(ApiErrorConstants.ErrorMessages.ResourceNotFound, "Product", id),
                    string.Format(ApiErrorConstants.ErrorDetails.ResourceDoesNotExist, "product"))
            ).ToApiResult(System.Net.HttpStatusCode.NotFound);
        }

        return new ApiResponse<Product>(product).ToApiResult();
    }
}
