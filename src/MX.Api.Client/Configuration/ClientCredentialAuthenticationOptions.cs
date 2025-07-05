namespace MX.Api.Client.Configuration;

/// <summary>
/// Authentication options for Entra ID using client credentials (client ID and secret).
/// </summary>
public class ClientCredentialAuthenticationOptions : EntraIdAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the tenant (directory) ID for authentication.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client (application) ID for authentication.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for authentication.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}
