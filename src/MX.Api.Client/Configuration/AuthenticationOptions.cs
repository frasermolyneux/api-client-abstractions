namespace MX.Api.Client.Configuration;

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
