namespace MX.Api.IntegrationTests.DummyApis.WeatherApi.Models;

/// <summary>
/// Weather forecast model
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// The date of the forecast
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Temperature in Fahrenheit
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// Weather summary
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Location of the forecast
    /// </summary>
    public string Location { get; set; } = string.Empty;
}
