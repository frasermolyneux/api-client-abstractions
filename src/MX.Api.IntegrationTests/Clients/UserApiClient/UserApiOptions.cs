using MX.Api.Client.Configuration;

namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// Custom options for the User API client with additional features
/// </summary>
public class UserApiOptions : ApiClientOptionsBase
{
    /// <summary>
    /// Whether to enable user caching
    /// </summary>
    public bool EnableUserCaching { get; set; } = false;

    /// <summary>
    /// Cache expiration time in minutes
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Whether to enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Default role for new users
    /// </summary>
    public string DefaultUserRole { get; set; } = "User";

    /// <summary>
    /// Maximum page size for user queries
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}
