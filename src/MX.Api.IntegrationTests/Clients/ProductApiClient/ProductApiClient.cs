namespace MX.Api.IntegrationTests.Clients.ProductApiClient;

/// <summary>
/// Product API client implementation using versioned approach
/// </summary>
/// <remarks>
/// Initializes a new instance of ProductApiClient
/// </remarks>
public class ProductApiClient(IVersionedProductApi versionedProductApi) : IProductApiClient
{

    /// <summary>
    /// Gets the versioned Product API
    /// </summary>
    public IVersionedProductApi Products { get; } = versionedProductApi;
}
