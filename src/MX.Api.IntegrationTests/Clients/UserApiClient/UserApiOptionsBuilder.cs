using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;

namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// Custom options builder for the User API client
/// </summary>
public class UserApiOptionsBuilder : ApiClientOptionsBuilder<UserApiOptions, UserApiOptionsBuilder>
{
    /// <summary>
    /// Creates a new instance of the UserApiOptionsBuilder
    /// </summary>
    public UserApiOptionsBuilder() : base() { }

    /// <summary>
    /// Enables user caching with specified expiration time
    /// </summary>
    /// <param name="expirationMinutes">Cache expiration time in minutes</param>
    /// <returns>The builder instance for method chaining</returns>
    public UserApiOptionsBuilder WithUserCaching(int expirationMinutes = 30)
    {
        Options.EnableUserCaching = true;
        Options.CacheExpirationMinutes = expirationMinutes;
        return this;
    }

    /// <summary>
    /// Enables detailed logging
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public UserApiOptionsBuilder WithDetailedLogging()
    {
        Options.EnableDetailedLogging = true;
        return this;
    }

    /// <summary>
    /// Sets the default role for new users
    /// </summary>
    /// <param name="role">Default user role</param>
    /// <returns>The builder instance for method chaining</returns>
    public UserApiOptionsBuilder WithDefaultRole(string role)
    {
        ArgumentException.ThrowIfNullOrEmpty(role);
        Options.DefaultUserRole = role;
        return this;
    }

    /// <summary>
    /// Sets the maximum page size for user queries
    /// </summary>
    /// <param name="maxPageSize">Maximum page size</param>
    /// <returns>The builder instance for method chaining</returns>
    public UserApiOptionsBuilder WithMaxPageSize(int maxPageSize)
    {
        if (maxPageSize <= 0)
            throw new ArgumentException("Max page size must be greater than 0", nameof(maxPageSize));

        Options.MaxPageSize = maxPageSize;
        return this;
    }

    /// <summary>
    /// Configures basic authentication with API token
    /// </summary>
    /// <param name="apiToken">The API token</param>
    /// <returns>The builder instance for method chaining</returns>
    public UserApiOptionsBuilder WithBasicAuth(string apiToken)
    {
        var apiKeyOptions = new ApiKeyAuthenticationOptions();
        apiKeyOptions.SetApiKey(apiToken);
        return this.WithAuthentication(apiKeyOptions);
    }

    /// <summary>
    /// Configures with test-friendly defaults
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public UserApiOptionsBuilder WithTestDefaults()
    {
        return this.WithUserCaching(30)
                   .WithDetailedLogging()
                   .WithDefaultRole("Member")
                   .WithMaxPageSize(50);
    }
}
