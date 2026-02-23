namespace MX.Api.Client.Configuration;

/// <summary>
/// Base builder for configuring API client options in a fluent manner
/// </summary>
/// <typeparam name="TOptions">The type of options being built</typeparam>
/// <typeparam name="TBuilder">The builder implementation type</typeparam>
public abstract class ApiClientOptionsBuilder<TOptions, TBuilder>
    where TOptions : ApiClientOptionsBase, new()
    where TBuilder : ApiClientOptionsBuilder<TOptions, TBuilder>
{
    /// <summary>
    /// The options instance being configured
    /// </summary>
    protected readonly TOptions Options;

    /// <summary>
    /// Initializes a new instance of the builder
    /// </summary>
    protected ApiClientOptionsBuilder()
    {
        Options = new TOptions();
    }

    /// <summary>
    /// Sets the base URL for API requests
    /// </summary>
    /// <param name="baseUrl">The base URL of the API</param>
    /// <returns>The builder instance for method chaining</returns>
    public TBuilder WithBaseUrl(string baseUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);
        Options.BaseUrl = baseUrl;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the maximum retry count for failed API calls
    /// </summary>
    /// <param name="maxRetryCount">The maximum number of retry attempts</param>
    /// <returns>The builder instance for method chaining</returns>
    public TBuilder WithMaxRetryCount(int maxRetryCount)
    {
        Options.MaxRetryCount = maxRetryCount;
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an authentication option to the collection
    /// </summary>
    /// <param name="authenticationOptions">The authentication options to add</param>
    /// <returns>The builder instance for method chaining</returns>
    public TBuilder WithAuthentication(AuthenticationOptions authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(authenticationOptions);
        Options.AuthenticationOptions.Add(authenticationOptions);
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds API key authentication
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication</param>
    /// <param name="headerName">The header or query parameter name to use for the API key. Defaults to "Ocp-Apim-Subscription-Key".</param>
    /// <param name="location">Where to place the API key in the request. Defaults to <see cref="ApiKeyLocation.Header"/>.</param>
    /// <returns>The builder instance for method chaining</returns>
    public TBuilder WithApiKeyAuthentication(string apiKey, string headerName = "Ocp-Apim-Subscription-Key", ApiKeyLocation location = ApiKeyLocation.Header)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        ArgumentException.ThrowIfNullOrEmpty(headerName);

        var apiKeyOptions = new ApiKeyAuthenticationOptions
        {
            HeaderName = headerName,
            Location = location
        };
        apiKeyOptions.SetApiKey(apiKey);
        Options.AuthenticationOptions.Add(apiKeyOptions);
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a subscription key for Azure API Management
    /// </summary>
    /// <param name="subscriptionKey">The subscription key value</param>
    /// <param name="headerName">The header name (default: "Ocp-Apim-Subscription-Key")</param>
    /// <returns>The builder instance for method chaining</returns>
    public TBuilder WithSubscriptionKey(string subscriptionKey, string headerName = "Ocp-Apim-Subscription-Key")
    {
        return WithApiKeyAuthentication(subscriptionKey, headerName);
    }

    /// <summary>
    /// Adds Entra ID authentication
    /// </summary>
    /// <param name="apiAudience">The API audience/scope for Entra ID authentication</param>
    /// <returns>The builder instance for method chaining</returns>
    public TBuilder WithEntraIdAuthentication(string apiAudience)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiAudience);

        Options.AuthenticationOptions.Add(new AzureCredentialAuthenticationOptions { ApiAudience = apiAudience });
        return (TBuilder)this;
    }

    /// <summary>
    /// Builds the options instance
    /// </summary>
    /// <returns>The configured options</returns>
    public TOptions Build()
    {
        Options.Validate();
        return Options;
    }
}

/// <summary>
/// Standard builder for configuring ApiClientOptions in a fluent manner
/// </summary>
public class ApiClientOptionsBuilder : ApiClientOptionsBuilder<ApiClientOptions, ApiClientOptionsBuilder>
{
    /// <summary>
    /// Creates a new instance of the ApiClientOptionsBuilder
    /// </summary>
    public ApiClientOptionsBuilder() : base() { }
}
