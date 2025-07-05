namespace MX.Api.Client.Configuration;

/// <summary>
/// Base class for all Entra ID authentication options.
/// </summary>
public abstract class EntraIdAuthenticationOptions : AuthenticationOptions
{
    /// <summary>
    /// Gets or sets the API audience value for token acquisition.
    /// </summary>
    public string ApiAudience { get; set; } = string.Empty;

    /// <summary>
    /// Gets the type of authentication.
    /// </summary>
    public override AuthenticationType AuthenticationType => AuthenticationType.EntraId;
}
