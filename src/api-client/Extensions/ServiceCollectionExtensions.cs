using Microsoft.Extensions.DependencyInjection;

namespace MxIO.ApiClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApiClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();
            serviceCollection.AddSingleton<IRestClientSingleton, RestClientSingleton>();
        }
    }
}