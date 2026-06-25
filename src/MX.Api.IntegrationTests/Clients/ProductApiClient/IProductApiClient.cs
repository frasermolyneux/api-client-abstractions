namespace MX.Api.IntegrationTests.Clients.ProductApiClient;

/// <summary>
/// Interface for Product API client
/// </summary>
public interface IProductApiClient
{
    /// <summary>
    /// Gets the versioned Product API
    /// </summary>
    IVersionedProductApi Products { get; }
}
