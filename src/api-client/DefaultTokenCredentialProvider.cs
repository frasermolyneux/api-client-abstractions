using Azure.Core;
using Azure.Identity;

namespace MxIO.ApiClient
{
    /// <summary>
    /// Default implementation of ITokenCredentialProvider that uses DefaultAzureCredential.
    /// </summary>
    public class DefaultTokenCredentialProvider : ITokenCredentialProvider
    {
        /// <summary>
        /// Gets a DefaultAzureCredential instance for authentication.
        /// </summary>
        /// <returns>A DefaultAzureCredential instance.</returns>
        public TokenCredential GetTokenCredential()
        {
            return new DefaultAzureCredential();
        }
    }
}