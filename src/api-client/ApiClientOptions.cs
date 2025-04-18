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
    public string PrimaryApiKey
    {
        get => _primaryApiKey;
        set => _primaryApiKey = value;
    }

    /// <summary>
    /// Gets or sets the secondary API key used as a fallback for authentication.
    /// </summary>
    public string SecondaryApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API audience value for token acquisition.
    /// </summary>
    public string ApiAudience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional path prefix to append to the base URL.
    /// </summary>
    public string? ApiPathPrefix { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClientOptions"/> class.
    /// </summary>
    public ApiClientOptions()
    {
    }
}
