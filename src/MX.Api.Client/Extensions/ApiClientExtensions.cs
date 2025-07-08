using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;
using Polly;
using Polly.Extensions.Http;

namespace MX.Api.Client.Extensions;

/// <summary>
/// Extension methods for registering API clients with dependency injection.
/// </summary>
public static class ApiClientExtensions
{
    /// <summary>
    /// Adds a typed API client to the service collection with default options configuration.
    /// This is a simplified version of AddTypedApiClient that uses default ApiClientOptions and ApiClientOptionsBuilder.
    /// </summary>
    /// <typeparam name="TClient">The interface type of the API client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the API client.</typeparam>
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configureOptions">An action to configure the API client options.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
    /// <example>
    /// <code>
    /// // Register with interface for better testability
    /// services.AddApiClient&lt;IMyApiClient, MyApiClient&gt;(options =&gt;
    /// {
    ///     options.WithBaseUrl("https://api.example.com")
    ///            .WithApiKeyAuthentication("your-api-key");
    /// });
    /// 
    /// // Use in your application
    /// public class UserService
    /// {
    ///     private readonly IMyApiClient _apiClient;
    ///     
    ///     public UserService(IMyApiClient apiClient)
    ///     {
    ///         _apiClient = apiClient;
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddApiClient<TClient, TImplementation>(
        this IServiceCollection services,
        Action<ApiClientOptionsBuilder> configureOptions)
        where TClient : class
        where TImplementation : class, TClient
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return services.AddTypedApiClient<TClient, TImplementation, ApiClientOptions, ApiClientOptionsBuilder>(configureOptions);
    }

    /// <summary>
    /// Adds a strongly-typed API client with custom options and builder types.
    /// This method provides maximum flexibility by allowing custom options and builder types,
    /// making it suitable for complex scenarios with specialized authentication or configuration requirements.
    /// </summary>
    /// <typeparam name="TClient">The interface type of the API client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the API client.</typeparam>
    /// <typeparam name="TOptions">The custom options type for this client, must inherit from ApiClientOptionsBase.</typeparam>
    /// <typeparam name="TBuilder">The custom builder type for configuring options, must inherit from ApiClientOptionsBuilder.</typeparam>
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configureOptions">An action to configure the client options using the custom builder.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null.</exception>
    /// <example>
    /// <code>
    /// // Define custom options and builder for specialized scenarios
    /// public class CustomApiOptions : ApiClientOptionsBase
    /// {
    ///     public string CustomProperty { get; set; }
    /// }
    /// 
    /// public class CustomApiOptionsBuilder : ApiClientOptionsBuilder&lt;CustomApiOptions, CustomApiOptionsBuilder&gt;
    /// {
    ///     public CustomApiOptionsBuilder WithCustomProperty(string value)
    ///     {
    ///         Options.CustomProperty = value;
    ///         return this;
    ///     }
    /// }
    /// 
    /// // Register the client with custom options
    /// services.AddTypedApiClient&lt;IMyApiClient, MyApiClient, CustomApiOptions, CustomApiOptionsBuilder&gt;(options =&gt;
    /// {
    ///     options.WithBaseUrl("https://api.example.com")
    ///            .WithApiKeyAuthentication("your-api-key")
    ///            .WithCustomProperty("custom-value");
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddTypedApiClient<TClient, TImplementation, TOptions, TBuilder>(
        this IServiceCollection services,
        Action<TBuilder> configureOptions)
        where TClient : class
        where TImplementation : class, TClient
        where TOptions : ApiClientOptionsBase, new()
        where TBuilder : ApiClientOptionsBuilder<TOptions, TBuilder>, new()
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Add base API client services if not already added
        services.AddMemoryCache();
        services.AddLogging();
        services.AddScoped<IRestClientService, RestClientService>();

        // Create and configure options using the builder
        var builder = new TBuilder();
        configureOptions(builder);
        var options = builder.Build();

        // Register the options as a singleton
        services.AddSingleton(options);
        services.AddSingleton<IOptions<TOptions>>(new OptionsWrapper<TOptions>(options));

        // Register API token provider if using Entra ID authentication
        var entraIdOptions = options.AuthenticationOptions.OfType<EntraIdAuthenticationOptions>().ToList();
        if (entraIdOptions.Any() && !services.Any(sd => sd.ServiceType == typeof(IApiTokenProvider)))
        {
            // Ensure that IMemoryCache is registered
            services.AddMemoryCache();

            // Register the appropriate token credential provider based on the first Entra ID option
            var firstEntraIdOption = entraIdOptions.First();

            // Register the token credential provider if using Azure credentials
            if (firstEntraIdOption is AzureCredentialAuthenticationOptions)
            {
                services.AddSingleton<ITokenCredentialProvider, DefaultTokenCredentialProvider>();
            }
            // Register the token credential provider if using client credentials
            else if (firstEntraIdOption is ClientCredentialAuthenticationOptions clientCredOptions)
            {
                services.AddSingleton<ITokenCredentialProvider>(sp =>
                {
                    var logger = sp.GetService<ILogger<ClientCredentialProvider>>();
                    return new ClientCredentialProvider(logger, clientCredOptions);
                });
            }

            // Register the API token provider for all Entra ID authentication types
            services.AddSingleton<IApiTokenProvider, ApiTokenProvider>();
        }

        // Register the client with HttpClientFactory and policy handlers
        services.AddHttpClient(typeof(TImplementation).Name, client =>
        {
            // Pre-configure HttpClient if needed
            client.BaseAddress = !string.IsNullOrEmpty(options.BaseUrl) ? new Uri(options.BaseUrl) : null;
        })
            .AddPolicyHandler((provider, _) =>
            {
                var maxRetryCount = options.MaxRetryCount > 0 ? options.MaxRetryCount : 3;

                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        maxRetryCount,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            });

        // Register the client with a factory that creates it with all required dependencies
        services.AddTransient<TClient>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<BaseApi<TOptions>>();
            var apiTokenProvider = serviceProvider.GetService<IApiTokenProvider>();
            var restClientService = serviceProvider.GetRequiredService<IRestClientService>();
            var clientOptions = serviceProvider.GetRequiredService<TOptions>();

            return (TClient)Activator.CreateInstance(typeof(TImplementation), logger, apiTokenProvider, restClientService, clientOptions)!;
        });

        return services;
    }
}
