using Azure.Core;
using Azure.Identity;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MxIO.ApiClient;

public class ApiTokenProvider : IApiTokenProvider
{
    private readonly ILogger<ApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;
    private readonly ITokenCredentialProvider tokenCredentialProvider;

    public ApiTokenProvider(ILogger<ApiTokenProvider> logger, IMemoryCache memoryCache, ITokenCredentialProvider tokenCredentialProvider)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        this.tokenCredentialProvider = tokenCredentialProvider;
    }

    public async Task<string> GetAccessToken(string audience, CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(audience, out AccessToken accessToken))
        {
            if (DateTime.UtcNow < accessToken.ExpiresOn)
                return accessToken.Token;
        }

        var tokenCredential = tokenCredentialProvider.GetTokenCredential();

        try
        {
            accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{audience}/.default" }),
                cancellationToken);

            memoryCache.Set(audience, accessToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to get identity token from AAD for audience: '{audience}'");
            throw;
        }

        return accessToken.Token;
    }
}