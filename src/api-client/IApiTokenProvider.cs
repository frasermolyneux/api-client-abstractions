namespace MxIO.ApiClient;

/// <summary>
/// Interface for providing access tokens for API authentication.
/// </summary>
public interface IApiTokenProvider
{
    /// <summary>
    /// Gets an access token for the specified audience.
    /// </summary>
    /// <param name="audience">The audience for which the token is requested.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
    /// <returns>A task representing the asynchronous operation, containing the access token as a string.</returns>
    Task<string> GetAccessToken(string audience, CancellationToken cancellationToken = default);
}