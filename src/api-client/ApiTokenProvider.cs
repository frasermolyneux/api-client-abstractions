using Azure.Core;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MxIO.ApiClient;

/// <summary>
/// Default implementation of IApiTokenProvider that manages access tokens with caching.
/// </summary>
public class ApiTokenProvider : IApiTokenProvider
{
    private const string DefaultScopeFormat = "{0}/.default";
    private static readonly TimeSpan DefaultExpiryBuffer = TimeSpan.FromMinutes(5);

    private readonly ILogger<ApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;
    private readonly ITokenCredentialProvider tokenCredentialProvider;
    private readonly TimeSpan tokenExpiryBuffer;

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
        : this(logger, memoryCache, tokenCredentialProvider, DefaultExpiryBuffer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiTokenProvider"/> class with a custom token expiry buffer.
    /// </summary>
    /// <param name="logger">The logger for recording diagnostic information.</param>
    /// <param name="memoryCache">The memory cache for storing acquired tokens.</param>
    /// <param name="tokenCredentialProvider">The provider for token credentials.</param>
    /// <param name="tokenExpiryBuffer">Time buffer before token expiry to consider a token expired.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if tokenExpiryBuffer is negative.</exception>
    public ApiTokenProvider(
        ILogger<ApiTokenProvider> logger,
        IMemoryCache memoryCache,
        ITokenCredentialProvider tokenCredentialProvider,
        TimeSpan tokenExpiryBuffer)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        this.tokenCredentialProvider = tokenCredentialProvider ?? throw new ArgumentNullException(nameof(tokenCredentialProvider));

        if (tokenExpiryBuffer < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenExpiryBuffer), "Token expiry buffer must be non-negative");
        }

        this.tokenExpiryBuffer = tokenExpiryBuffer;
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
            DateTimeOffset.UtcNow < accessToken.ExpiresOn.Subtract(tokenExpiryBuffer))
        {
            logger.LogDebug("Using cached token for audience '{Audience}' which expires at {ExpiryTime} (effective: {EffectiveExpiry})",
                audience, accessToken.ExpiresOn, accessToken.ExpiresOn.Subtract(tokenExpiryBuffer));
            return accessToken.Token;
        }

        // Get a new token
        TokenCredential? tokenCredential = null;
        try
        {
            tokenCredential = tokenCredentialProvider.GetTokenCredential();
            ArgumentNullException.ThrowIfNull(tokenCredential);

            // Cancel operation if requested
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogDebug("Requesting new token for audience '{Audience}'", audience);

            // Request a new token with the appropriate scope format
            accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { string.Format(DefaultScopeFormat, audience) }),
                cancellationToken);

            // Calculate effective expiry with buffer
            var effectiveExpiry = accessToken.ExpiresOn.Subtract(tokenExpiryBuffer);

            // Cache the token for future use
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = accessToken.ExpiresOn,
                Priority = CacheItemPriority.High // Token access is critical
            };

            memoryCache.Set(audience, accessToken, cacheOptions);

            logger.LogDebug("Acquired and cached new token for audience '{Audience}' that expires at {ExpiryTime} (effective: {EffectiveExpiry})",
                audience, accessToken.ExpiresOn, effectiveExpiry);

            return accessToken.Token;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Token acquisition for audience '{Audience}' was canceled", audience);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get identity token for audience: '{Audience}'", audience);
            throw;
        }
    }
}