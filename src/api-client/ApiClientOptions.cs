namespace MxIO.ApiClient;

/// <summary>
/// Options for configuring the API client.
/// </summary>
public class ApiClientOptions
{
    private string _primaryApiKey = string.Empty;

    /// <summary>
    /// Gets or sets the base URL of the API.
    /// </summary>
    /// <remarks>This property is required for the API client to function correctly.</remarks>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary API key used for authentication.
    /// </summary>
    [Obsolete("This property is obsolete, please use PrimaryApiKey instead")]
    public string ApiKey
    {
        get => _primaryApiKey;
        set => _primaryApiKey = value;
    }

    /// <summary>
    /// Gets or sets the primary API key used for authentication.
    /// </summary>
    /// <remarks>This property is required for the API client to function correctly.</remarks>
    public string PrimaryApiKey
    {
        get => _primaryApiKey;
        set => _primaryApiKey = value;
    }

    /// <summary>
    /// Gets or sets the secondary API key used as a fallback for authentication.
    /// </summary>
    /// <remarks>This is optional and will be used as a fallback if the primary key fails.</remarks>
    public string SecondaryApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API audience value for token acquisition.
    /// </summary>
    /// <remarks>This property is required for the API client to function correctly.</remarks>
    public string ApiAudience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional path prefix to append to the base URL.
    /// </summary>
    /// <remarks>
    /// When provided, this value will be appended to the BaseUrl with appropriate
    /// handling of slashes to form the complete endpoint URL.
    /// </remarks>
    public string? ApiPathPrefix { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed API calls.
    /// </summary>
    /// <remarks>
    /// When not set or set to a value less than or equal to 0, a default of 3 retry attempts will be used.
    /// </remarks>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClientOptions"/> class.
    /// </summary>
    public ApiClientOptions()
    {
        // Default constructor
    }
}
