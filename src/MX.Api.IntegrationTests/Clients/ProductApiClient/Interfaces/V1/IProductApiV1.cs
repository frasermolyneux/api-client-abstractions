using MX.Api.Abstractions;
using MX.Api.IntegrationTests.DummyApis.ProductApi.Models;

namespace MX.Api.IntegrationTests.Clients.ProductApiClient.Interfaces.V1;

/// <summary>
/// Interface for Product API version 1
/// </summary>
public interface IProductApiV1
{
    /// <summary>
    /// Get all products
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products</returns>
    Task<ApiResult<CollectionModel<Product>>> GetProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product or error</returns>
    Task<ApiResult<Product>> GetProductAsync(int id, CancellationToken cancellationToken = default);
}
