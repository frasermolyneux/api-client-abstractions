using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;

namespace MX.Api.Client.Testing;

/// <summary>
/// Extension methods for registering test doubles of API clients in dependency injection.
/// These methods simplify setting up API clients for unit tests, integration tests, and UI tests
/// without making actual HTTP calls.
/// </summary>
public static class TestingExtensions
{
    /// <summary>
    /// Registers an API client with in-memory test doubles that don't make real HTTP calls.
    /// Perfect for unit testing, integration testing, and UI testing scenarios.
    /// </summary>
    /// <typeparam name="TClient">The interface type of the API client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the API client.</typeparam>
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configureOptions">An action to configure the API client options.</param>
    /// <param name="configureTestService">Optional action to configure the in-memory test service with predefined responses.</param>
    /// <returns>The in-memory rest client service for further configuration.</returns>
    /// <example>
    /// <code>
    /// // Register a test API client
    /// var testService = services.AddTestApiClient&lt;IMyApiClient, MyApiClient&gt;(
    ///     options => options.WithBaseUrl("https://test.example.com"),
    ///     testService =>
    ///     {
    ///         testService.AddResponse("users/123", new RestResponse
    ///         {
    ///             StatusCode = HttpStatusCode.OK,
    ///             Content = "{\"id\": \"123\", \"name\": \"John Doe\"}"
    ///         });
    ///     });
    /// 
    /// // Later in your test, you can verify calls were made
    /// Assert.True(testService.WasCalled("users/123"));
    /// </code>
    /// </example>
    public static InMemoryRestClientService AddTestApiClient<TClient, TImplementation>(
        this IServiceCollection services,
        Action<ApiClientOptionsBuilder> configureOptions,
        Action<InMemoryRestClientService>? configureTestService = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services.AddTestTypedApiClient<TClient, TImplementation, ApiClientOptions, ApiClientOptionsBuilder>(
            configureOptions,
            configureTestService);
    }

    /// <summary>
    /// Registers a strongly-typed API client with custom options and in-memory test doubles.
    /// This method provides maximum flexibility for complex testing scenarios.
    /// </summary>
    /// <typeparam name="TClient">The interface type of the API client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the API client.</typeparam>
    /// <typeparam name="TOptions">The custom options type for this client.</typeparam>
    /// <typeparam name="TBuilder">The custom builder type for configuring options.</typeparam>
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configureOptions">An action to configure the client options.</param>
    /// <param name="configureTestService">Optional action to configure the in-memory test service.</param>
    /// <returns>The in-memory rest client service for further configuration.</returns>
    public static InMemoryRestClientService AddTestTypedApiClient<TClient, TImplementation, TOptions, TBuilder>(
        this IServiceCollection services,
        Action<TBuilder> configureOptions,
        Action<InMemoryRestClientService>? configureTestService = null)
        where TClient : class
        where TImplementation : class, TClient
        where TOptions : ApiClientOptionsBase, new()
        where TBuilder : ApiClientOptionsBuilder<TOptions, TBuilder>, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Add base services
        services.AddMemoryCache();
        services.AddLogging();

        // Create and configure the in-memory test service
        var inMemoryService = new InMemoryRestClientService();
        configureTestService?.Invoke(inMemoryService);

        // Register the in-memory service as a singleton
        services.TryAddSingleton<IRestClientService>(inMemoryService);

        // Create and configure options
        var builder = new TBuilder();
        configureOptions(builder);
        var options = builder.Build();

        // Register the options
        services.AddSingleton(options);
        services.AddSingleton<IOptions<TOptions>>(new OptionsWrapper<TOptions>(options));

        // Register fake token provider if Entra ID authentication is configured
        var entraIdOptions = options.AuthenticationOptions.OfType<EntraIdAuthenticationOptions>().ToList();
        if (entraIdOptions.Any())
        {
            var fakeTokenProvider = new FakeApiTokenProvider();
            
            // Set up fake tokens for each audience
            foreach (var entraOption in entraIdOptions)
            {
                fakeTokenProvider.SetToken(entraOption.ApiAudience, $"fake-token-for-{entraOption.ApiAudience}");
            }

            services.TryAddSingleton<IApiTokenProvider>(fakeTokenProvider);
        }

        // Register the client
        services.AddTransient<TClient>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<BaseApi<TOptions>>();
            var apiTokenProvider = serviceProvider.GetService<IApiTokenProvider>();
            var restClientService = serviceProvider.GetRequiredService<IRestClientService>();
            var clientOptions = serviceProvider.GetRequiredService<TOptions>();

            return (TClient)Activator.CreateInstance(typeof(TImplementation), logger, apiTokenProvider, restClientService, clientOptions)!;
        });

        return inMemoryService;
    }

    /// <summary>
    /// Replaces the IRestClientService registration with an in-memory implementation.
    /// Useful when you want to add test doubles to an existing service collection.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configureTestService">Optional action to configure the test service.</param>
    /// <returns>The in-memory rest client service for further configuration.</returns>
    /// <example>
    /// <code>
    /// // Replace the real service with a test double
    /// var testService = services.UseInMemoryRestClientService(testService =>
    /// {
    ///     testService.AddResponse("api/health", new RestResponse 
    ///     { 
    ///         StatusCode = HttpStatusCode.OK 
    ///     });
    /// });
    /// </code>
    /// </example>
    public static InMemoryRestClientService UseInMemoryRestClientService(
        this IServiceCollection services,
        Action<InMemoryRestClientService>? configureTestService = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var inMemoryService = new InMemoryRestClientService();
        configureTestService?.Invoke(inMemoryService);

        // Remove any existing IRestClientService registrations
        var existingRegistrations = services.Where(sd => sd.ServiceType == typeof(IRestClientService)).ToList();
        foreach (var registration in existingRegistrations)
        {
            services.Remove(registration);
        }

        // Register the in-memory service
        services.AddSingleton<IRestClientService>(inMemoryService);

        return inMemoryService;
    }

    /// <summary>
    /// Replaces the IApiTokenProvider registration with a fake implementation.
    /// Useful when you want to test authentication scenarios without real credentials.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configureTokenProvider">Optional action to configure the fake token provider.</param>
    /// <returns>The fake token provider for further configuration.</returns>
    /// <example>
    /// <code>
    /// var fakeTokenProvider = services.UseFakeApiTokenProvider(provider =>
    /// {
    ///     provider.SetToken("api://users", "test-user-token");
    ///     provider.SetToken("api://orders", "test-order-token");
    /// });
    /// </code>
    /// </example>
    public static FakeApiTokenProvider UseFakeApiTokenProvider(
        this IServiceCollection services,
        Action<FakeApiTokenProvider>? configureTokenProvider = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var fakeProvider = new FakeApiTokenProvider();
        configureTokenProvider?.Invoke(fakeProvider);

        // Remove any existing IApiTokenProvider registrations
        var existingRegistrations = services.Where(sd => sd.ServiceType == typeof(IApiTokenProvider)).ToList();
        foreach (var registration in existingRegistrations)
        {
            services.Remove(registration);
        }

        // Register the fake provider
        services.AddSingleton<IApiTokenProvider>(fakeProvider);

        return fakeProvider;
    }
}
