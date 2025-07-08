using MX.Api.Abstractions;
using MX.Api.IntegrationTests.DummyApis.WeatherApi.Models;

namespace MX.Api.IntegrationTests.Clients.WeatherApiClient;

/// <summary>
/// Interface for the Weather API client
/// </summary>
public interface IWeatherApiClient
{
    /// <summary>
    /// Gets weather forecasts for a location
    /// </summary>
    /// <param name="location">The location to get forecasts for</param>
    /// <param name="days">Number of days to forecast</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Weather forecasts</returns>
    Task<ApiResult<IEnumerable<WeatherForecast>>> GetForecastAsync(
        string location = "London",
        int days = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current weather for a location
    /// </summary>
    /// <param name="location">The location to get current weather for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current weather</returns>
    Task<ApiResult<WeatherForecast>> GetCurrentWeatherAsync(
        string location = "London",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a health check
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status</returns>
    Task<ApiResult<string>> HealthCheckAsync(CancellationToken cancellationToken = default);
}
