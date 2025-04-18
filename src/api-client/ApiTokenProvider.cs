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
    /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
    public ApiTokenProvider(
        ILogger<ApiTokenProvider> logger,
        IMemoryCache memoryCache,
        ITokenCredentialProvider tokenCredentialProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        this.tokenCredentialProvider = tokenCredentialProvider ?? throw new ArgumentNullException(nameof(tokenCredentialProvider));
    }

    /// <summary>
    /// Gets an access token for the specified audience, using cached tokens when available.
    /// </summary>
    /// <param name="audience">The audience for which the token is requested.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the request.</param>
    /// <returns>The access token string.</returns>
    /// <exception cref="ArgumentException">Thrown when the audience is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="Exception">Thrown when token acquisition fails.</exception>
    public async Task<string> GetAccessToken(string audience, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(audience))
        {
            throw new ArgumentException("Audience cannot be null or empty", nameof(audience));
        }

        // Check if we already have a valid cached token
        if (memoryCache.TryGetValue(audience, out AccessToken accessToken) &&
            DateTime.UtcNow < accessToken.ExpiresOn)
        {
            logger.LogDebug("Using cached token for audience '{Audience}' which expires at {ExpiryTime}",
                audience, accessToken.ExpiresOn);
            return accessToken.Token;
        }

        // Get a new token
        var tokenCredential = tokenCredentialProvider.GetTokenCredential();
        ArgumentNullException.ThrowIfNull(tokenCredential);

        try
        {
            // Cancel operation if requested
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogDebug("Requesting new token for audience '{Audience}'", audience);

            // Request a new token with the appropriate scope format
            accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{audience}/.default" }),
                cancellationToken);

            // Cache the token for future use with a sliding expiration
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = accessToken.ExpiresOn
            };

            memoryCache.Set(audience, accessToken, cacheOptions);

            logger.LogDebug("Acquired and cached new token for audience '{Audience}' that expires at {ExpiryTime}",
                audience, accessToken.ExpiresOn);

            return accessToken.Token;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Token acquisition for audience '{Audience}' was canceled", audience);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get identity token from AAD for audience: '{Audience}'", audience);
            throw;
        }
    }
}