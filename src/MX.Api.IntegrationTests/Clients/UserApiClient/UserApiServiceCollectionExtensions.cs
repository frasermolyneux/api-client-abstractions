using Microsoft.Extensions.DependencyInjection;
using MX.Api.Client.Extensions;
using MX.Api.IntegrationTests.Clients.UserApiClient.Api.V1;
using MX.Api.IntegrationTests.Clients.UserApiClient.Interfaces.V1;

namespace MX.Api.IntegrationTests.Clients.UserApiClient;

/// <summary>
/// Extension methods for registering the User API client
/// </summary>
public static class UserApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds the User API client with simplified configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="baseUrl">The base URL of the User API</param>
    /// <param name="apiToken">The API token for authentication</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUserApiClient(
        this IServiceCollection services,
        string baseUrl,
        string apiToken)
    {
        services.AddTypedApiClient<IUserApiV1, UserApiV1, UserApiOptions, UserApiOptionsBuilder>(builder => builder
            .WithBaseUrl(baseUrl)
            .WithApiKeyAuthentication(apiToken)
            .WithUserCaching(30)
            .WithDetailedLogging()
            .WithDefaultRole("Member")
            .WithMaxPageSize(50));

        // Register versioned API wrapper
        services.AddScoped<IVersionedUserApi, VersionedUserApi>();

        // Register main client
        services.AddScoped<IUserApiClient, UserApiClient>();

        return services;
    }

    /// <summary>
    /// Adds the User API client with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the User API options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUserApiClient(
        this IServiceCollection services,
        Action<UserApiOptionsBuilder> configureOptions)
    {
        services.AddTypedApiClient<IUserApiV1, UserApiV1, UserApiOptions, UserApiOptionsBuilder>(configureOptions);

        // Build options using builder pattern
        var builder = new UserApiOptionsBuilder();
        configureOptions(builder);
        var options = builder.Build();

        services.Configure<UserApiOptions>(opts =>
        {
            opts.BaseUrl = options.BaseUrl;
            opts.EnableUserCaching = options.EnableUserCaching;
            opts.EnableDetailedLogging = options.EnableDetailedLogging;
            opts.CacheExpirationMinutes = options.CacheExpirationMinutes;
            opts.DefaultUserRole = options.DefaultUserRole;
            opts.MaxPageSize = options.MaxPageSize;
        });

        // Register versioned API wrapper
        services.AddScoped<IVersionedUserApi, VersionedUserApi>();

        // Register main client
        services.AddScoped<IUserApiClient, UserApiClient>();

        return services;
    }
}
