namespace MX.Api.Client.Configuration;

/// <summary>
/// Authentication options for API Key authentication.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationOptions
{
    /// <summary>
    /// Gets or sets the API key used for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the header name for the API key. Defaults to "Ocp-Apim-Subscription-Key" for Azure API Management.
    /// </summary>
    public string HeaderName { get; set; } = "Ocp-Apim-Subscription-Key";

    /// <summary>
    /// Gets the type of authentication.
    /// </summary>
    public override AuthenticationType AuthenticationType => AuthenticationType.ApiKey;
}
