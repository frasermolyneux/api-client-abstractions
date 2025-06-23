using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace MxIO.ApiClient.Extensions;

/// <summary>
/// Extension methods for configuring Entra ID authentication.
/// </summary>
public static class EntraIdAuthenticationExtensions
{
    /// <summary>
    /// Configures an API client with Entra ID authentication using Azure credentials (DefaultAzureCredential).
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <param name="apiAudience">The API audience value for token acquisition.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection WithAzureCredentials(
        this IServiceCollection serviceCollection,
        string apiAudience)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        // Ensure that IMemoryCache is registered
        serviceCollection.AddMemoryCache();

        // Register the token credential provider
        serviceCollection.AddSingleton<ITokenCredentialProvider, DefaultTokenCredentialProvider>();

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            options.AuthenticationOptions = new AzureCredentialAuthenticationOptions
            {
                ApiAudience = apiAudience
            };
        });

        return serviceCollection;
    }

    /// <summary>
    /// Configures an API client with Entra ID authentication using Azure credentials with custom DefaultAzureCredentialOptions.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <param name="apiAudience">The API audience value for token acquisition.</param>
    /// <param name="configureCredentialOptions">An action to configure the DefaultAzureCredentialOptions.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
    public static IServiceCollection WithAzureCredentials(
        this IServiceCollection serviceCollection,
        string apiAudience,
        Action<DefaultAzureCredentialOptions> configureCredentialOptions)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(configureCredentialOptions);

        // Configure Azure credential options
        serviceCollection.Configure(configureCredentialOptions);

        // Ensure that IMemoryCache is registered
        serviceCollection.AddMemoryCache();

        // Register the token credential provider with configured options
        serviceCollection.AddSingleton<ITokenCredentialProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DefaultAzureCredentialOptions>>();
            var logger = sp.GetService<ILogger<DefaultTokenCredentialProvider>>();
            return new DefaultTokenCredentialProvider(logger, options);
        });

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            options.AuthenticationOptions = new AzureCredentialAuthenticationOptions
            {
                ApiAudience = apiAudience
            };
        });

        return serviceCollection;
    }

    /// <summary>
    /// Configures an API client with Entra ID authentication using client credentials (client ID and secret).
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <param name="apiAudience">The API audience value for token acquisition.</param>
    /// <param name="tenantId">The tenant (directory) ID of the application registration.</param>
    /// <param name="clientId">The client (application) ID of the application registration.</param>
    /// <param name="clientSecret">The client secret of the application registration.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if any required string parameter is empty.</exception>
    public static IServiceCollection WithClientCredentials(
        this IServiceCollection serviceCollection,
        string apiAudience,
        string tenantId,
        string clientId,
        string clientSecret)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        if (string.IsNullOrEmpty(apiAudience))
            throw new ArgumentException("API audience cannot be null or empty", nameof(apiAudience));

        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));

        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));

        if (string.IsNullOrEmpty(clientSecret))
            throw new ArgumentException("Client secret cannot be null or empty", nameof(clientSecret));

        // Ensure that IMemoryCache is registered
        serviceCollection.AddMemoryCache();

        // Register the token credential provider
        serviceCollection.AddSingleton<ITokenCredentialProvider>(sp =>
        {
            var logger = sp.GetService<ILogger<ClientCredentialProvider>>();
            return new ClientCredentialProvider(logger, tenantId, clientId, clientSecret);
        });

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            options.AuthenticationOptions = new ClientCredentialAuthenticationOptions
            {
                ApiAudience = apiAudience,
                TenantId = tenantId,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
        });

        return serviceCollection;
    }
}
