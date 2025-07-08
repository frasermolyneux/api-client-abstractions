using MX.Api.Client.Configuration;

namespace MX.Api.IntegrationTests.Clients.WeatherApiClient;

/// <summary>
/// Enhanced options builder for the Weather API client
/// </summary>
public class WeatherApiOptionsBuilder : ApiClientOptionsBuilder<ApiClientOptions, WeatherApiOptionsBuilder>
{
    /// <summary>
    /// Creates a new instance of the WeatherApiOptionsBuilder
    /// </summary>
    public WeatherApiOptionsBuilder() : base() { }

    /// <summary>
    /// Configures with test-friendly defaults for Weather API
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public WeatherApiOptionsBuilder WithTestDefaults()
    {
        return this.WithMaxRetryCount(3);
    }
}
