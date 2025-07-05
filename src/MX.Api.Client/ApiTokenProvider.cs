using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;

namespace MX.Api.Client;

/// <summary>
/// A simplified token provider implementation for API authentication.
/// </summary>
public class ApiTokenProvider : IApiTokenProvider
{
    private readonly ILogger<ApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;
    private readonly ITokenCredentialProvider tokenCredentialProvider;
    private readonly TimeSpan tokenExpiryBuffer;

    private const string DefaultScopeFormat = "{0}/.default";
    private static readonly TimeSpan DefaultExpiryBuffer = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiTokenProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger for recording diagnostic information.</param>
    /// <param name="memoryCache">The memory cache for storing acquired tokens.</param>
    /// <param name="tokenCredentialProvider">The provider for token credentials.</param>
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
    /// <exception cref="AuthenticationException">Thrown when token acquisition fails.</exception>
    public async Task<string> GetAccessTokenAsync(string audience, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(audience))
        {
            throw new ArgumentException("Audience cannot be null or empty", nameof(audience));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var cacheKey = $"access_token:{audience}";

        // Try to get token from cache
        if (memoryCache.TryGetValue(cacheKey, out string? cachedToken) && cachedToken != null)
        {
            logger.LogDebug("Using cached token for audience '{Audience}'", audience);
            return cachedToken;
        }

        // Get new token
        try
        {
            var credential = await tokenCredentialProvider.GetTokenCredentialAsync(cancellationToken);
            var tokenResult = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { string.Format(DefaultScopeFormat, audience) }),
                cancellationToken);

            // Cache token with buffer time
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = tokenResult.ExpiresOn.Subtract(tokenExpiryBuffer),
                Priority = CacheItemPriority.High
            };

            memoryCache.Set(cacheKey, tokenResult.Token, cacheOptions);

            logger.LogDebug("Acquired and cached new token for audience '{Audience}' that expires at {ExpiryTime}",
                audience, tokenResult.ExpiresOn);

            return tokenResult.Token;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get identity token for audience: '{Audience}'", audience);
            throw new AuthenticationException($"Failed to acquire authentication token for audience: '{audience}'", ex);
        }
    }
}
