using System.Collections.Generic;

namespace MX.Api.Client.Configuration;

/// <summary>
/// Base options class for all API clients
/// </summary>
public abstract class ApiClientOptionsBase
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
    public IList<AuthenticationOptions> AuthenticationOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed API calls.
    /// </summary>
    /// <remarks>
    /// When not set or set to a value less than or equal to 0, a default of 3 retry attempts will be used.
    /// The retry mechanism uses exponential backoff with a base of 2 seconds.
    /// </remarks>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Validates the options configuration
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public virtual void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new ArgumentException("BaseUrl must be provided", nameof(BaseUrl));
        }
    }
}
