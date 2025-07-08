using Microsoft.Extensions.DependencyInjection;
using MX.Api.Client.Extensions;
using MX.Api.IntegrationTests.Clients.ProductApiClient.Api.V1;
using MX.Api.IntegrationTests.Clients.ProductApiClient.Interfaces.V1;

namespace MX.Api.IntegrationTests.Clients.ProductApiClient;

/// <summary>
/// Extension methods for registering the Product API client
/// </summary>
public static class ProductApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Product API client with simplified configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="baseUrl">The base URL of the Product API</param>
    /// <param name="bearerToken">The Bearer token for authentication</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddProductApiClient(
        this IServiceCollection services,
        string baseUrl,
        string bearerToken)
    {
        services.AddTypedApiClient<IProductApiV1, ProductApiV1, ProductApiOptions, ProductApiOptionsBuilder>(builder => builder
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(bearerToken));

        // Configure custom options
        services.Configure<ProductApiOptions>(options =>
        {
            options.BaseUrl = baseUrl;
            options.MaxRetryCount = 3;
        });

        // Register versioned API wrapper
        services.AddScoped<IVersionedProductApi, VersionedProductApi>();

        // Register main client
        services.AddScoped<IProductApiClient, ProductApiClient>();

        return services;
    }

    /// <summary>
    /// Adds the Product API client with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the Product API options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddProductApiClient(
        this IServiceCollection services,
        Action<ProductApiOptionsBuilder> configureOptions)
    {
        services.AddTypedApiClient<IProductApiV1, ProductApiV1, ProductApiOptions, ProductApiOptionsBuilder>(configureOptions);

        // Build options using builder pattern
        var builder = new ProductApiOptionsBuilder();
        configureOptions(builder);
        var options = builder.Build();

        services.Configure<ProductApiOptions>(opts =>
        {
            opts.BaseUrl = options.BaseUrl;
            opts.MaxRetryCount = options.MaxRetryCount;
        });

        // Register versioned API wrapper
        services.AddScoped<IVersionedProductApi, VersionedProductApi>();

        // Register main client
        services.AddScoped<IProductApiClient, ProductApiClient>();

        return services;
    }
}
