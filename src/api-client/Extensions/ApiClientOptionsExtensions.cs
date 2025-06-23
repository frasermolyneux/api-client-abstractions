using Microsoft.Extensions.Options;

namespace MxIO.ApiClient.Extensions;

/// <summary>
/// Extension methods for API client options.
/// </summary>
public static class ApiClientOptionsExtensions
{
    /// <summary>
    /// Sets or updates the API key for an existing API client configuration.
    /// This is useful for dynamically updating the API key at runtime.
    /// </summary>
    /// <param name="options">The API client options to update.</param>
    /// <param name="apiKey">The new API key value.</param>
    /// <returns>The same options instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the authentication type is not API key.</exception>
    public static ApiClientOptions UpdateApiKey(this ApiClientOptions options, string apiKey)
    {
        if (options.AuthenticationOptions is not ApiKeyAuthenticationOptions apiKeyOptions)
        {
            throw new InvalidOperationException("Cannot update API key: authentication is not configured for API key");
        }

        apiKeyOptions.ApiKey = apiKey;
        return options;
    }

    /// <summary>
    /// Sets or updates the API audience for an existing API client configuration with Entra ID authentication.
    /// </summary>
    /// <param name="options">The API client options to update.</param>
    /// <param name="apiAudience">The new API audience value.</param>
    /// <returns>The same options instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the authentication type is not Entra ID.</exception>
    public static ApiClientOptions UpdateApiAudience(this ApiClientOptions options, string apiAudience)
    {
        if (options.AuthenticationOptions is not EntraIdAuthenticationOptions entraIdOptions)
        {
            throw new InvalidOperationException("Cannot update API audience: authentication is not configured for Entra ID");
        }

        entraIdOptions.ApiAudience = apiAudience;
        return options;
    }
}
