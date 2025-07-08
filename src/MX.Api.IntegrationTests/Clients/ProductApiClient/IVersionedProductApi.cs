using MX.Api.IntegrationTests.Clients.ProductApiClient.Interfaces.V1;

namespace MX.Api.IntegrationTests.Clients.ProductApiClient;

/// <summary>
/// Interface for versioned Product API client
/// </summary>
public interface IVersionedProductApi
{
    /// <summary>
    /// Gets the V1 Product API
    /// </summary>
    IProductApiV1 V1 { get; }
}
