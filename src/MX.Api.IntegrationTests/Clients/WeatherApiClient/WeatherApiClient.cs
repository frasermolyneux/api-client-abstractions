using System.Globalization;
using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using MX.Api.IntegrationTests.DummyApis.WeatherApi.Models;
using RestSharp;

namespace MX.Api.IntegrationTests.Clients.WeatherApiClient;

/// <summary>
/// Weather API client implementation using the standard ApiClientOptions
/// </summary>
/// <remarks>
/// Initializes a new instance of the WeatherApiClient
/// </remarks>
public class WeatherApiClient(
    ILogger<BaseApi<ApiClientOptions>> logger,
    IApiTokenProvider? apiTokenProvider,
    IRestClientService restClientService,
    ApiClientOptions options) : BaseApi<ApiClientOptions>(logger, apiTokenProvider, restClientService, options), IWeatherApiClient
{

    /// <inheritdoc />
    public async Task<ApiResult<IEnumerable<WeatherForecast>>> GetForecastAsync(
        string location = "London",
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("api/weather/forecast", Method.Get, cancellationToken);
        _ = request.AddQueryParameter("location", location);
        _ = request.AddQueryParameter("days", days.ToString(CultureInfo.InvariantCulture));

        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<IEnumerable<WeatherForecast>>();
    }

    /// <inheritdoc />
    public async Task<ApiResult<WeatherForecast>> GetCurrentWeatherAsync(
        string location = "London",
        CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("api/weather/current", Method.Get, cancellationToken);
        _ = request.AddQueryParameter("location", location);

        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<WeatherForecast>();
    }

    /// <inheritdoc />
    public async Task<ApiResult<string>> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var request = await CreateRequestAsync("api/weather/health", Method.Get, cancellationToken);
        var response = await ExecuteAsync(request, cancellationToken);
        return response.ToApiResult<string>();
    }
}
