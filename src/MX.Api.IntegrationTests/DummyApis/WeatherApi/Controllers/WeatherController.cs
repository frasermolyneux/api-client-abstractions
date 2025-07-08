using Microsoft.AspNetCore.Mvc;
using MX.Api.Abstractions;
using MX.Api.IntegrationTests.Constants;
using MX.Api.IntegrationTests.Clients.WeatherApiClient;
using MX.Api.IntegrationTests.DummyApis.WeatherApi.Models;
using MX.Api.Web.Extensions;

namespace MX.Api.IntegrationTests.DummyApis.WeatherApi.Controllers;

/// <summary>
/// Weather API controller for testing purposes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase, IWeatherApiClient
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherController> _logger;

    /// <summary>
    /// Initializes a new instance of the WeatherController
    /// </summary>
    public WeatherController(ILogger<WeatherController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets weather forecasts for a location
    /// </summary>
    /// <param name="location">The location to get forecasts for</param>
    /// <param name="days">Number of days to forecast (default: 5)</param>
    /// <returns>Weather forecasts</returns>
    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast(
        [FromQuery] string location = "London",
        [FromQuery] int days = 5)
    {
        var result = await ((IWeatherApiClient)this).GetForecastAsync(location, days);
        return result.ToHttpResult();
    }

    /// <summary>
    /// Gets current weather for a location
    /// </summary>
    /// <param name="location">The location to get current weather for</param>
    /// <returns>Current weather</returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentWeather([FromQuery] string location = "London")
    {
        var result = await ((IWeatherApiClient)this).GetCurrentWeatherAsync(location);
        return result.ToHttpResult();
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        var result = await ((IWeatherApiClient)this).HealthCheckAsync();
        return result.ToHttpResult();
    }

    #region IWeatherApiClient Implementation

    /// <summary>
    /// Gets weather forecasts for a location
    /// </summary>
    /// <param name="location">The location to get forecasts for</param>
    /// <param name="days">Number of days to forecast</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Weather forecasts</returns>
    Task<ApiResult<IEnumerable<WeatherForecast>>> IWeatherApiClient.GetForecastAsync(
        string location,
        int days,
        CancellationToken cancellationToken)
    {
        // Simple API key validation
        if (!Request.Headers.ContainsKey("X-API-Key"))
        {
            var unauthorizedResponse = new ApiResponse<IEnumerable<WeatherForecast>>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.ApiKeyRequired,
                    ApiErrorConstants.ErrorDetails.XApiKeyHeaderMissing));
            return Task.FromResult(unauthorizedResponse.ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        var apiKey = Request.Headers["X-API-Key"].ToString();
        if (apiKey != "weather-test-key")
        {
            var invalidKeyResponse = new ApiResponse<IEnumerable<WeatherForecast>>(
                new ApiError(ApiErrorConstants.ErrorCodes.InvalidApiKey,
                    ApiErrorConstants.ErrorMessages.InvalidApiKey,
                    ApiErrorConstants.ErrorDetails.InvalidApiKeyProvided));
            return Task.FromResult(invalidKeyResponse.ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        _logger.LogInformation("Getting weather forecast for {Location} for {Days} days", location, days);

        var forecasts = Enumerable.Range(1, Math.Min(days, 10)).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            Location = location
        });

        var response = new ApiResponse<IEnumerable<WeatherForecast>>(forecasts);
        return Task.FromResult(response.ToApiResult());
    }

    /// <summary>
    /// Gets current weather for a location
    /// </summary>
    /// <param name="location">The location to get current weather for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current weather</returns>
    Task<ApiResult<WeatherForecast>> IWeatherApiClient.GetCurrentWeatherAsync(
        string location,
        CancellationToken cancellationToken)
    {
        // Simple API key validation
        if (!Request.Headers.ContainsKey("X-API-Key"))
        {
            var unauthorizedResponse = new ApiResponse<WeatherForecast>(
                new ApiError(ApiErrorConstants.ErrorCodes.Unauthorized,
                    ApiErrorConstants.ErrorMessages.ApiKeyRequired,
                    ApiErrorConstants.ErrorDetails.XApiKeyHeaderMissing));
            return Task.FromResult(unauthorizedResponse.ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        var apiKey = Request.Headers["X-API-Key"].ToString();
        if (apiKey != "weather-test-key")
        {
            var invalidKeyResponse = new ApiResponse<WeatherForecast>(
                new ApiError(ApiErrorConstants.ErrorCodes.InvalidApiKey,
                    ApiErrorConstants.ErrorMessages.InvalidApiKey,
                    ApiErrorConstants.ErrorDetails.InvalidApiKeyProvided));
            return Task.FromResult(invalidKeyResponse.ToApiResult(System.Net.HttpStatusCode.Unauthorized));
        }

        _logger.LogInformation("Getting current weather for {Location}", location);

        var current = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            Location = location
        };

        var response = new ApiResponse<WeatherForecast>(current);
        return Task.FromResult(response.ToApiResult());
    }

    /// <summary>
    /// Performs a health check
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status</returns>
    Task<ApiResult<string>> IWeatherApiClient.HealthCheckAsync(CancellationToken cancellationToken)
    {
        var response = new ApiResponse<string>("Weather API is healthy");
        return Task.FromResult(response.ToApiResult());
    }

    #endregion
}
