using System;

namespace MxIO.ApiClient.V2;

/// <summary>
/// Base class for all authentication options.
/// </summary>
public abstract class AuthenticationOptions
{
    /// <summary>
    /// Gets the type of authentication.
    /// </summary>
    public abstract AuthenticationType AuthenticationType { get; }
}

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

/// <summary>
/// Authentication options for Entra ID using Azure credentials (Managed Identity, environment variables, etc.).
/// </summary>
public class AzureCredentialAuthenticationOptions : EntraIdAuthenticationOptions
{
}

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
