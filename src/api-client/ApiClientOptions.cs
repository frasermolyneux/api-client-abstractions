namespace MxIO.ApiClient;

/// <summary>
/// Options for configuring the API client.
/// </summary>
/// <remarks>
/// This class provides configuration options for API clients to connect to REST APIs.
/// It includes settings for base URLs, path prefixes, and retry policies.
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
    /// Gets or sets the authentication options for this API client.
    /// Can be null if no authentication is required.
    /// </summary>
    public AuthenticationOptions? AuthenticationOptions { get; set; }    /// <summary>
                                                                         /// Gets or sets an optional path prefix to append to the base URL.
                                                                         /// </summary>
                                                                         /// <remarks>
                                                                         /// When provided, this value will be appended to the BaseUrl with appropriate
                                                                         /// handling of slashes to form the complete endpoint URL.
                                                                         /// For example, if BaseUrl is "https://api.example.com" and ApiPathPrefix is "v1",
                                                                         /// the resulting URL used for requests will be "https://api.example.com/v1".
                                                                         /// </remarks>
    public string? ApiPathPrefix { get; set; }

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
}
