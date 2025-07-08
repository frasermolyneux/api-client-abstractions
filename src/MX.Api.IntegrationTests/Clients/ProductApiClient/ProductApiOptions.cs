using MX.Api.Client.Configuration;

namespace MX.Api.IntegrationTests.Clients.ProductApiClient;

/// <summary>
/// Configuration options for Product API client
/// </summary>
public class ProductApiOptions : ApiClientOptionsBase
{
}

/// <summary>
/// Builder for Product API options
/// </summary>
public class ProductApiOptionsBuilder : ApiClientOptionsBuilder<ProductApiOptions, ProductApiOptionsBuilder>
{
    /// <summary>
    /// Initializes a new instance of ProductApiOptionsBuilder
    /// </summary>
    public ProductApiOptionsBuilder() : base()
    {
    }

    /// <summary>
    /// Configures with test-friendly defaults for Product API
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public ProductApiOptionsBuilder WithTestDefaults()
    {
        return this.WithMaxRetryCount(3);
    }
}
