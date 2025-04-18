using Microsoft.Extensions.DependencyInjection;

namespace MxIO.ApiClient.Extensions;

/// <summary>
/// Extension methods for registering API client services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the required API client services with the service collection.
    /// This adds the following services:
    /// - <see cref="ITokenCredentialProvider"/> as a singleton
    /// - <see cref="IApiTokenProvider"/> as a singleton
    /// - <see cref="IRestClientSingleton"/> as a singleton
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the services to.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serviceCollection is null.</exception>
    public static IServiceCollection AddApiClient(this IServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        // Register the token credential provider
        serviceCollection.AddSingleton<ITokenCredentialProvider, DefaultTokenCredentialProvider>();

        // Register the API token provider
        serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

        // Register the REST client singleton
        serviceCollection.AddSingleton<IRestClientSingleton, RestClientSingleton>();

        return serviceCollection;
    }
}