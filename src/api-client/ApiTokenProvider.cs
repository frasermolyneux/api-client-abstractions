using Azure.Core;
using Azure.Identity;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MxIO.ApiClient;

public class ApiTokenProvider : IApiTokenProvider
{
    private readonly ILogger<ApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;

    private readonly string apiAudience;

    public ApiTokenProvider(ILogger<ApiTokenProvider> logger, IMemoryCache memoryCache, IOptions<IApiClientOptions> options)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;

        apiAudience = options.Value.ApiAudience;
    }

    public async Task<string> GetAccessToken()
    {
        if (memoryCache.TryGetValue(apiAudience, out AccessToken accessToken))
        {
            if (DateTime.UtcNow < accessToken.ExpiresOn)
                return accessToken.Token;
        }

        var tokenCredential = new DefaultAzureCredential();

        try
        {
            accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { $"{apiAudience}/.default" }));
            memoryCache.Set(apiAudience, accessToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to get identity token from AAD for audience: '{apiAudience}'");
            throw;
        }

        return accessToken.Token;
    }
}