namespace MX.Api.IntegrationTests.Clients.ProductApiClient;

/// <summary>
/// Product API client implementation using versioned approach
/// </summary>
public class ProductApiClient : IProductApiClient
{
    /// <summary>
    /// Initializes a new instance of ProductApiClient
    /// </summary>
    public ProductApiClient(IVersionedProductApi versionedProductApi)
    {
        Products = versionedProductApi;
    }

    /// <summary>
    /// Gets the versioned Product API
    /// </summary>
    public IVersionedProductApi Products { get; }
}
