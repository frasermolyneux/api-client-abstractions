using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;

namespace MX.Api.Client.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register and configure API client services.
/// </summary>
public static class ServiceCollectionExtensions
{    /// <summary>
     /// Registers the required API client services with the service collection.
     /// </summary>
     /// <remarks>
     /// This method registers the following core services:
     /// <list type="bullet">
     ///   <item><description><see cref="IRestClientService"/> as a singleton</description></item>
     ///   <item><description><see cref="IMemoryCache"/> if not already registered</description></item>
     ///   <item><description>HttpClientFactory for connection pooling</description></item>
     /// </list>
     /// </remarks>
     /// <param name="serviceCollection">The service collection to add the services to.</param>
     /// <returns>The same service collection for method chaining.</returns>
     /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection AddApiClient(this IServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        // Ensure that IMemoryCache is registered
        serviceCollection.AddMemoryCache();

        // Register the REST client service
        serviceCollection.AddSingleton<IRestClientService, RestClientService>();

        return serviceCollection;
    }

    /// <summary>
    /// Configures all API client options at once using an instance of <see cref="ApiClientOptions"/> or an action to configure it.
    /// </summary>
    /// <param name="serviceCollection">The service collection to configure.</param>
    /// <param name="optionsOrConfigureAction">Either an instance of ApiClientOptions or an action to configure the options.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection WithOptions(
        this IServiceCollection serviceCollection,
        object optionsOrConfigureAction)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(optionsOrConfigureAction);

        if (optionsOrConfigureAction is ApiClientOptions options)
        {
            // Configuring with an existing options instance
            serviceCollection.Configure<ApiClientOptions>(o =>
            {
                o.BaseUrl = options.BaseUrl;
                o.AuthenticationOptions = options.AuthenticationOptions;
                o.ApiPathPrefix = options.ApiPathPrefix;
                o.MaxRetryCount = options.MaxRetryCount;
            });

            // Register token providers if using Entra ID authentication
            if (options.AuthenticationOptions is EntraIdAuthenticationOptions)
            {
                // Ensure that IMemoryCache is registered
                serviceCollection.AddMemoryCache();

                // Register the token credential provider if using Azure credentials
                if (options.AuthenticationOptions is AzureCredentialAuthenticationOptions)
                {
                    serviceCollection.AddSingleton<ITokenCredentialProvider, DefaultTokenCredentialProvider>();
                }
                // Register the token credential provider if using client credentials
                else if (options.AuthenticationOptions is ClientCredentialAuthenticationOptions clientCredOptions)
                {
                    serviceCollection.AddSingleton<ITokenCredentialProvider>(sp =>
                    {
                        var logger = sp.GetService<ILogger<ClientCredentialProvider>>();
                        return new ClientCredentialProvider(logger, clientCredOptions);
                    });
                }

                // Register the API token provider for all Entra ID authentication types
                serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();
            }
        }
        else if (optionsOrConfigureAction is Action<ApiClientOptions> configureAction)
        {
            // Configuring with an action
            serviceCollection.Configure<ApiClientOptions>(configureAction);

            // Since we can't determine the authentication type here, 
            // let the caller register any required additional services
        }
        else
        {
            throw new ArgumentException(
                "Parameter must be either an ApiClientOptions instance or an Action<ApiClientOptions>",
                nameof(optionsOrConfigureAction));
        }

        return serviceCollection;
    }

    /// <summary>
    /// Configures the base URL for the API client.
    /// </summary>
    /// <param name="serviceCollection">The service collection to configure.</param>
    /// <param name="baseUrl">The base URL of the API.</param>
    /// <param name="configureOptions">Optional action to configure additional options.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    /// <exception cref="ArgumentException">Thrown if baseUrl is null or empty.</exception>
    public static IServiceCollection WithBaseUrl(
        this IServiceCollection serviceCollection,
        string baseUrl,
        Action<ApiClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            options.BaseUrl = baseUrl;

            // Apply any additional configuration
            configureOptions?.Invoke(options);
        });

        return serviceCollection;
    }

    /// <summary>
    /// Configures the API client to use API key authentication.
    /// </summary>
    /// <param name="serviceCollection">The service collection to configure.</param>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="headerName">The header name to use for the API key. Defaults to "Ocp-Apim-Subscription-Key".</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection WithApiKeyAuthentication(
        this IServiceCollection serviceCollection,
        string apiKey,
        string headerName = "Ocp-Apim-Subscription-Key")
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            var apiKeyOptions = new ApiKeyAuthenticationOptions
            {
                HeaderName = headerName
            };
            apiKeyOptions.SetApiKey(apiKey);
            options.AuthenticationOptions = apiKeyOptions;
        });

        return serviceCollection;
    }

    /// <summary>
    /// Configures the API client to use Azure credentials for authentication.
    /// </summary>
    /// <remarks>
    /// This method uses the DefaultAzureCredential to authenticate with Azure services.
    /// It automatically registers the necessary services for token acquisition and caching.
    /// </remarks>
    /// <param name="serviceCollection">The service collection to configure.</param>
    /// <param name="apiAudience">The API audience (resource) to request tokens for.</param>
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
    /// Configures the API client to use Azure credentials for authentication with custom credential options.
    /// </summary>
    /// <remarks>
    /// This method allows customization of the DefaultAzureCredential options while 
    /// automatically registering the necessary services for token acquisition and caching.
    /// </remarks>
    /// <param name="serviceCollection">The service collection to configure.</param>
    /// <param name="apiAudience">The API audience (resource) to request tokens for.</param>
    /// <param name="configureCredentialOptions">An action to configure the DefaultAzureCredentialOptions.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection or configureCredentialOptions is null.</exception>
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
    /// Configures the API client to use client credentials flow for authentication.
    /// </summary>
    /// <remarks>
    /// This method registers the necessary services for client credentials authentication
    /// using a tenant ID, client ID, and client secret. It automatically handles token acquisition
    /// and caching for improved performance.
    /// </remarks>
    /// <param name="serviceCollection">The service collection to configure.</param>
    /// <param name="apiAudience">The API audience (resource) to request tokens for.</param>
    /// <param name="tenantId">The Azure AD tenant ID.</param>
    /// <param name="clientId">The client (application) ID.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    /// <exception cref="ArgumentException">Thrown if apiAudience, tenantId, clientId, or clientSecret is null or empty.</exception>
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

            // Create secure options object
            var clientCredOptions = new ClientCredentialAuthenticationOptions
            {
                ApiAudience = apiAudience,
                TenantId = tenantId,
                ClientId = clientId
            };
            clientCredOptions.SetClientSecret(clientSecret);

            return new ClientCredentialProvider(logger, clientCredOptions);
        });

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            var clientCredOptions = new ClientCredentialAuthenticationOptions
            {
                ApiAudience = apiAudience,
                TenantId = tenantId,
                ClientId = clientId
            };
            clientCredOptions.SetClientSecret(clientSecret);
            options.AuthenticationOptions = clientCredOptions;
        });

        return serviceCollection;
    }
}