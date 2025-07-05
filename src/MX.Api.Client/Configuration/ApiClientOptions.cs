namespace MX.Api.Client.Configuration;

/// <summary>
/// Options for configuring the API client.
/// </summary>
/// <remarks>
/// This class provides configuration options for API clients to connect to REST APIs.
/// It includes settings for base URLs and retry policies.
/// Authentication settings are configured using the fluent API in ServiceCollectionExtensions.
/// </remarks>
public class ApiClientOptions
{
    /// <summary>
    /// Gets or sets the base URL of the API.
    /// </summary>
    /// <remarks>This property is required for the API client to function correctly.</remarks>
    /// <exception cref="ArgumentException">Thrown when this property is not set or is empty when used.</exception>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of authentication options for this API client.
    /// Multiple authentication methods can be applied in the order they are configured.
    /// Can be empty if no authentication is required.
    /// </summary>
    public IList<AuthenticationOptions> AuthenticationOptions { get; set; } = new List<AuthenticationOptions>();



    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed API calls.
    /// </summary>
    /// <remarks>
    /// When not set or set to a value less than or equal to 0, a default of 3 retry attempts will be used.
    /// The retry mechanism uses exponential backoff with a base of 2 seconds.
    /// </remarks>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClientOptions"/> class with default values.
    /// </summary>
    public ApiClientOptions()
    {
        // Default constructor
    }

    /// <summary>
    /// Creates a new instance with the specified base URL.
    /// </summary>
    /// <param name="baseUrl">The base URL of the API.</param>
    /// <returns>The API client options instance.</returns>
    public static ApiClientOptions Create(string baseUrl)
    {
        return new ApiClientOptions { BaseUrl = baseUrl };
    }

    /// <summary>
    /// Sets the base URL for this API client instance.
    /// </summary>
    /// <param name="baseUrl">The base URL of the API.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ApiClientOptions WithBaseUrl(string baseUrl)
    {
        BaseUrl = baseUrl;
        return this;
    }



    /// <summary>
    /// Sets the maximum retry count for failed API calls.
    /// </summary>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ApiClientOptions WithMaxRetryCount(int maxRetryCount)
    {
        MaxRetryCount = maxRetryCount;
        return this;
    }

    /// <summary>
    /// Adds an authentication option to the collection.
    /// Multiple authentication methods can be configured and will be applied in order.
    /// </summary>
    /// <param name="authenticationOptions">The authentication options to add.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ApiClientOptions WithAuthentication(AuthenticationOptions authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(authenticationOptions);
        AuthenticationOptions.Add(authenticationOptions);
        return this;
    }

    /// <summary>
    /// Convenience method to add API key authentication (typically for API Management subscription keys).
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="headerName">The header name to use for the API key. Defaults to "Ocp-Apim-Subscription-Key".</param>
    /// <returns>The current instance for method chaining.</returns>
    public ApiClientOptions WithApiKeyAuthentication(string apiKey, string headerName = "Ocp-Apim-Subscription-Key")
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        ArgumentException.ThrowIfNullOrEmpty(headerName);

        var apiKeyOptions = new ApiKeyAuthenticationOptions
        {
            HeaderName = headerName
        };
        apiKeyOptions.SetApiKey(apiKey);
        AuthenticationOptions.Add(apiKeyOptions);
        return this;
    }

    /// <summary>
    /// Convenience method to add a subscription key for Azure API Management.
    /// This is an alias for WithApiKeyAuthentication with the default APIM header.
    /// </summary>
    /// <param name="subscriptionKey">The subscription key value.</param>
    /// <param name="headerName">The header name (default: "Ocp-Apim-Subscription-Key").</param>
    /// <returns>The current instance for method chaining.</returns>
    public ApiClientOptions WithSubscriptionKey(string subscriptionKey, string headerName = "Ocp-Apim-Subscription-Key")
    {
        return WithApiKeyAuthentication(subscriptionKey, headerName);
    }

    /// <summary>
    /// Convenience method to add Entra ID authentication.
    /// </summary>
    /// <param name="apiAudience">The API audience/scope for Entra ID authentication.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ApiClientOptions WithEntraIdAuthentication(string apiAudience)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiAudience);

        AuthenticationOptions.Add(new AzureCredentialAuthenticationOptions { ApiAudience = apiAudience });
        return this;
    }
}
