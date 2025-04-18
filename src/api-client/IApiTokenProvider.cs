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
    Task<string> GetAccessTokenAsync(string audience, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an access token for the specified audience.
    /// </summary>
    /// <param name="audience">The audience for which the token is requested.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
    /// <returns>A task representing the asynchronous operation, containing the access token as a string.</returns>
    [Obsolete("This method has been renamed to GetAccessTokenAsync to follow async naming conventions. Use GetAccessTokenAsync instead.")]
    Task<string> GetAccessToken(string audience, CancellationToken cancellationToken = default);
}