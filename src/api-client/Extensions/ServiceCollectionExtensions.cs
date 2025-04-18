using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    /// This method registers the following services:
    /// <list type="bullet">
    ///   <item><description><see cref="ITokenCredentialProvider"/> as a singleton</description></item>
    ///   <item><description><see cref="IApiTokenProvider"/> as a singleton</description></item>
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

        // Ensure that IMemoryCache is registered, as it's required by ApiTokenProvider
        serviceCollection.AddMemoryCache();

        // Register the token credential provider
        serviceCollection.AddSingleton<ITokenCredentialProvider, DefaultTokenCredentialProvider>();

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        // Register the REST client singleton
        serviceCollection.AddSingleton<IRestClientSingleton, RestClientSingleton>();

        return serviceCollection;
    }
}