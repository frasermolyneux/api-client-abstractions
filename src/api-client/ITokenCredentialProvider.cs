using Azure.Core;

namespace MxIO.ApiClient
{
    /// <summary>
    /// Interface for providing token credentials.
    /// This abstraction allows for better testability by enabling mocking of token acquisition.
    /// </summary>
    public interface ITokenCredentialProvider
    {
        /// <summary>
        /// Gets a token credential that can be used for authentication.
        /// </summary>
        /// <returns>A token credential instance.</returns>
        TokenCredential GetTokenCredential();
    }
}