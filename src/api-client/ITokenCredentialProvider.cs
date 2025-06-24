using Azure.Core;

namespace MxIO.ApiClient.V2;

/// <summary>
/// Interface for providing token credentials.
/// This abstraction allows for better testability by enabling mocking of token acquisition.
/// </summary>
public interface ITokenCredentialProvider
{
    /// <summary>
    /// Gets a token credential asynchronously that can be used for authentication.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task containing a token credential instance.</returns>
    Task<TokenCredential> GetTokenCredentialAsync(CancellationToken cancellationToken = default);
}