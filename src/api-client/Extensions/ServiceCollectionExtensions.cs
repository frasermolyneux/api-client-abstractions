using Microsoft.Extensions.DependencyInjection;

namespace MxIO.ApiClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApiClientTokenProvider(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();
        }
    }
}