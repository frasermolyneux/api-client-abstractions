using Azure.Core;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MxIO.ApiClient;

/// <summary>
/// Default implementation of IApiTokenProvider that manages access tokens with caching.
/// </summary>
public class ApiTokenProvider : IApiTokenProvider
{
    private readonly ILogger<ApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;
    private readonly ITokenCredentialProvider tokenCredentialProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiTokenProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger for recording diagnostic information.</param>
    /// <param name="memoryCache">The memory cache for storing acquired tokens.</param>
    /// <param name="tokenCredentialProvider">The provider for token credentials.</param>
    public ApiTokenProvider(ILogger<ApiTokenProvider> logger, IMemoryCache memoryCache, ITokenCredentialProvider tokenCredentialProvider)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        this.tokenCredentialProvider = tokenCredentialProvider;
    }

    /// <summary>
    /// Gets an access token for the specified audience, using cached tokens when available.
    /// </summary>
    /// <param name="audience">The audience for which the token is requested.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the request.</param>
    /// <returns>The access token string.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="Exception">Thrown when token acquisition fails.</exception>
    public async Task<string> GetAccessToken(string audience, CancellationToken cancellationToken = default)
    {
        // Check if we already have a valid cached token
        if (memoryCache.TryGetValue(audience, out AccessToken accessToken))
        {
            if (DateTime.UtcNow < accessToken.ExpiresOn)
                return accessToken.Token;
        }

        // Get a new token
        var tokenCredential = tokenCredentialProvider.GetTokenCredential();

        try
        {
            // Cancel operation if requested
            cancellationToken.ThrowIfCancellationRequested();

            // Request a new token
            accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{audience}/.default" }),
                cancellationToken);

            // Cache the token for future use
            memoryCache.Set(audience, accessToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation($"Token acquisition for audience '{audience}' was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to get identity token from AAD for audience: '{audience}'");
            throw;
        }

        return accessToken.Token;
    }
}