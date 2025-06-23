using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MxIO.ApiClient.Extensions;

/// <summary>
/// Extension methods for registering API client services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the required API client services with the service collection.
    /// </summary>
    /// <remarks>
    /// This method registers the following core services:
    /// <list type="bullet">
    ///   <item><description><see cref="IRestClientSingleton"/> as a singleton</description></item>
    ///   <item><description><see cref="IMemoryCache"/> if not already registered</description></item>
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

        // Register the REST client singleton
        serviceCollection.AddSingleton<IRestClientSingleton, RestClientSingleton>();

        return serviceCollection;
    }

    /// <summary>
    /// Configures an API client with API key authentication for Azure API Management or similar services.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <param name="apiKey">The API key used for authentication.</param>
    /// <param name="headerName">Optional header name for the API key (defaults to "Ocp-Apim-Subscription-Key").</param>
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
            options.AuthenticationOptions = new ApiKeyAuthenticationOptions
            {
                ApiKey = apiKey,
                HeaderName = headerName
            };
        });

        return serviceCollection;
    }

    /// <summary>
    /// Configures an API client with Entra ID (formerly Azure AD) authentication.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <param name="apiAudience">The API audience value for token acquisition.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection WithEntraIdAuthentication(
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
            options.AuthenticationOptions = new EntraIdAuthenticationOptions
            {
                ApiAudience = apiAudience
            };
        });

        return serviceCollection;
    }

    /// <summary>
    /// Configures an API client with Entra ID (formerly Azure AD) authentication and custom DefaultAzureCredentialOptions.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <param name="apiAudience">The API audience value for token acquisition.</param>
    /// <param name="configureCredentialOptions">An action to configure the DefaultAzureCredentialOptions.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection WithEntraIdAuthentication(
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
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<DefaultTokenCredentialProvider>>();
            return new DefaultTokenCredentialProvider(logger, options);
        });

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            options.AuthenticationOptions = new EntraIdAuthenticationOptions
            {
                ApiAudience = apiAudience
            };
        });

        return serviceCollection;
    }

    /// <summary>
    /// Registers a custom implementation of ITokenCredentialProvider and configures Entra ID authentication.
    /// </summary>
    /// <typeparam name="TTokenCredentialProvider">The type of the custom token credential provider.</typeparam>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <param name="apiAudience">The API audience value for token acquisition.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection WithCustomCredentialProvider<TTokenCredentialProvider>(
        this IServiceCollection serviceCollection,
        string apiAudience)
        where TTokenCredentialProvider : class, ITokenCredentialProvider
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        // Ensure that IMemoryCache is registered, as it's required by SimpleApiTokenProvider
        serviceCollection.AddMemoryCache();

        // Register the custom token credential provider
        serviceCollection.AddSingleton<ITokenCredentialProvider, TTokenCredentialProvider>();

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        serviceCollection.Configure<ApiClientOptions>(options =>
        {
            options.AuthenticationOptions = new EntraIdAuthenticationOptions
            {
                ApiAudience = apiAudience
            };
        });

        // Register the REST client singleton
        serviceCollection.AddSingleton<IRestClientSingleton, RestClientSingleton>();

        return serviceCollection;
    }
}