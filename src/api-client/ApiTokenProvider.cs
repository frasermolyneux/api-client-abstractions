using Azure.Core;
using Azure.Identity;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MxIO.ApiClient;

public class ApiTokenProvider : IApiTokenProvider
{
    private readonly ILogger<ApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;

    public ApiTokenProvider(ILogger<ApiTokenProvider> logger, IMemoryCache memoryCache)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
    }

    public async Task<string> GetAccessToken(string audience)
    {
        if (memoryCache.TryGetValue(audience, out AccessToken accessToken))
        {
            if (DateTime.UtcNow < accessToken.ExpiresOn)
                return accessToken.Token;
        }

        var tokenCredential = new DefaultAzureCredential();

        try
        {
            accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { $"{audience}/.default" }));
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