using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.IntegrationTests.Clients.ProductApiClient.Interfaces.V1;
using MX.Api.IntegrationTests.DummyApis.ProductApi.Models;
using RestSharp;

namespace MX.Api.IntegrationTests.Clients.ProductApiClient.Api.V1;

/// <summary>
/// Implementation of Product API version 1
/// </summary>
public class ProductApiV1 : BaseApi<ProductApiOptions>, IProductApiV1
{
    public ProductApiV1(
        ILogger<BaseApi<ProductApiOptions>> logger,
        IApiTokenProvider apiTokenProvider,
        IRestClientService restClientService,
        ProductApiOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    /// <summary>
    /// Get all products
    /// </summary>
    public async Task<ApiResult<CollectionModel<Product>>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("v1/products", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);

        return response.ToApiResult<CollectionModel<Product>>();
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    public async Task<ApiResult<Product>> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync($"v1/products/{id}", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);

        return response.ToApiResult<Product>();
    }
}
