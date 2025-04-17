using Microsoft.Extensions.DependencyInjection;

namespace MxIO.ApiClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApiClient(this IServiceCollection serviceCollection)
        {
            // Register the token credential provider
            serviceCollection.AddSingleton<ITokenCredentialProvider, DefaultTokenCredentialProvider>();

            // Register the API token provider
            serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

            // Register the REST client singleton
            serviceCollection.AddSingleton<IRestClientSingleton, RestClientSingleton>();
        }
    }
}