using Microsoft.Extensions.DependencyInjection;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;

namespace MX.Api.IntegrationTests.Clients.WeatherApiClient;

/// <summary>
/// Extension methods for registering the Weather API client
/// </summary>
public static class WeatherApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Weather API client with simplified configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="baseUrl">The base URL of the Weather API</param>
    /// <param name="apiKey">The API key for authentication</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWeatherApiClient(
        this IServiceCollection services,
        string baseUrl,
        string apiKey)
    {
        return services.AddTypedApiClient<IWeatherApiClient, WeatherApiClient, ApiClientOptions, WeatherApiOptionsBuilder>(options =>
        {
            options.WithBaseUrl(baseUrl)
                   .WithApiKeyAuthentication(apiKey, "X-API-Key")
                   .WithTestDefaults();
        });
    }

    /// <summary>
    /// Adds the Weather API client with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the Weather API options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWeatherApiClient(
        this IServiceCollection services,
        Action<WeatherApiOptionsBuilder> configureOptions)
    {
        return services.AddTypedApiClient<IWeatherApiClient, WeatherApiClient, ApiClientOptions, WeatherApiOptionsBuilder>(configureOptions);
    }
}
