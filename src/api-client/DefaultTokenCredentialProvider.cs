using Azure.Core;
using Azure.Identity;

namespace MxIO.ApiClient;

/// <summary>
/// Default implementation of ITokenCredentialProvider that uses DefaultAzureCredential.
/// This provider is used to authenticate with Azure services using the DefaultAzureCredential,
/// which tries multiple authentication methods in sequence.
/// </summary>
public class DefaultTokenCredentialProvider : ITokenCredentialProvider
{
    /// <summary>
    /// Gets a DefaultAzureCredential instance for authentication.
    /// The DefaultAzureCredential attempts to authenticate via the following mechanisms in order:
    /// - Environment variables
    /// - Managed Identity
    /// - Visual Studio
    /// - Azure CLI
    /// - Azure PowerShell
    /// </summary>
    /// <returns>A DefaultAzureCredential instance for token acquisition.</returns>
    public TokenCredential GetTokenCredential()
    {
        return new DefaultAzureCredential();
    }
}