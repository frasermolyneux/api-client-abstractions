using MX.Api.IntegrationTests.Clients.ProductApiClient.Interfaces.V1;

namespace MX.Api.IntegrationTests.Clients.ProductApiClient;

/// <summary>
/// Implementation of versioned Product API client
/// </summary>
public class VersionedProductApi(IProductApiV1 v1Api) : IVersionedProductApi
{

    /// <summary>
    /// Gets the V1 Product API
    /// </summary>
    public IProductApiV1 V1 { get; } = v1Api;
}
