namespace MX.Api.Client.Configuration;

/// <summary>
/// Enum defining the available authentication types.
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    /// No authentication.
    /// </summary>
    None = 0,

    /// <summary>
    /// API key authentication.
    /// </summary>
    ApiKey = 1,

    /// <summary>
    /// Entra ID (formerly Azure AD) authentication.
    /// </summary>
    EntraId = 2
}
