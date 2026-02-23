namespace MX.Api.Client.Configuration;

/// <summary>
/// Specifies where an API key should be placed in the HTTP request.
/// </summary>
public enum ApiKeyLocation
{
    /// <summary>
    /// Send the API key as an HTTP header.
    /// </summary>
    Header = 0,

    /// <summary>
    /// Send the API key as a query string parameter.
    /// </summary>
    QueryParameter = 1
}
