using System.Security.Authentication;

namespace MxIO.ApiClient.V2;

/// <summary>
/// Interface for providing access tokens for API authentication.
/// </summary>
public interface IApiTokenProvider
{
    /// <summary>
    /// Gets an access token for the specified audience.
    /// </summary>
    /// <param name="audience">The audience for which the token is requested.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the access token as a string.</returns>
    /// <exception cref="ArgumentException">Thrown when audience is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="AuthenticationException">Thrown when token acquisition fails.</exception>
    Task<string> GetAccessTokenAsync(string audience, CancellationToken cancellationToken = default);
}