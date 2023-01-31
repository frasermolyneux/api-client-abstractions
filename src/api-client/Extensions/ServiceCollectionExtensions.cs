using Microsoft.Extensions.DependencyInjection;

namespace MX.ApiClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServersApiClient(this IServiceCollection serviceCollection, Action<ApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);
            serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();
        }
    }
}