using Azure.Core;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;

namespace MxIO.ApiClient;

/// <summary>
/// Default implementation of IApiTokenProvider that manages access tokens with caching.
/// This class is responsible for acquiring and caching access tokens for API authentication.
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
    /// <returns>The access token string.</returns>    /// <exception cref="ArgumentException">Thrown when the audience is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="AuthenticationException">Thrown when token acquisition fails.</exception>
    public async Task<string> GetAccessTokenAsync(string audience, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(audience))
        {
            throw new ArgumentException("Audience cannot be null or empty", nameof(audience));
        }

        // Check for cancellation before proceeding
        cancellationToken.ThrowIfCancellationRequested();

        // Check if we already have a valid cached token
        if (memoryCache.TryGetValue(audience, out AccessToken accessToken))
        {
            try
            {
                // Safely apply the expiry buffer with overflow protection
                DateTimeOffset effectiveExpiry = SafeSubtractBuffer(accessToken.ExpiresOn);

                if (DateTimeOffset.UtcNow < effectiveExpiry)
                {
                    logger.LogDebug("Using cached token for audience '{Audience}' which expires at {ExpiryTime} (effective: {EffectiveExpiry})",
                        audience, accessToken.ExpiresOn, effectiveExpiry);
                    return accessToken.Token;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error calculating token expiration for cached token. Will request a new token.");
                // Continue to get a new token
            }
        }

        // Get a new token
        try
        {
            // Get token credential using the async method
            TokenCredential tokenCredential = await tokenCredentialProvider.GetTokenCredentialAsync(cancellationToken);
            ArgumentNullException.ThrowIfNull(tokenCredential);

            logger.LogDebug("Requesting new token for audience '{Audience}'", audience);

            // Request a new token with the appropriate scope format
            accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { string.Format(DefaultScopeFormat, audience) }),
                cancellationToken);

            try
            {
                // Calculate effective expiry with buffer using the safe method
                var effectiveExpiry = SafeSubtractBuffer(accessToken.ExpiresOn);

                // Cache the token for future use
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = accessToken.ExpiresOn,
                    Priority = CacheItemPriority.High // Token access is critical
                };

                memoryCache.Set(audience, accessToken, cacheOptions);

                logger.LogDebug("Acquired and cached new token for audience '{Audience}' that expires at {ExpiryTime} (effective: {EffectiveExpiry})",
                    audience, accessToken.ExpiresOn, effectiveExpiry);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error calculating effective expiry time for token. Using token without caching.");
                // Still return the token even if we can't cache it properly
            }

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
            throw new AuthenticationException($"Failed to acquire authentication token for audience: '{audience}'", ex);
        }
    }

    /// <summary>
    /// Safely subtracts the token expiry buffer from the given DateTimeOffset, ensuring no underflow occurs.
    /// </summary>
    /// <param name="dateTimeOffset">The original DateTimeOffset.</param>
    /// <returns>The DateTimeOffset with the buffer subtracted, or the earliest valid date if underflow would occur.</returns>
    private DateTimeOffset SafeSubtractBuffer(DateTimeOffset dateTimeOffset)
    {
        try
        {
            return dateTimeOffset.Subtract(tokenExpiryBuffer);
        }
        catch (ArgumentOutOfRangeException)
        {
            logger.LogWarning("Could not subtract token expiry buffer from {ExpiresOn} as it would result in an invalid DateTime. Using minimum valid date instead.", dateTimeOffset);
            // Return the earliest possible date that's still valid
            return DateTimeOffset.MinValue.AddTicks(1);
        }
    }
}