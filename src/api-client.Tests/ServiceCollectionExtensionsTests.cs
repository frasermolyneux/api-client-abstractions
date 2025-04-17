using Microsoft.Extensions.DependencyInjection;
using MxIO.ApiClient.Extensions;
using System.Linq;
using Xunit;

namespace MxIO.ApiClient
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddApiClient_RegistersRequiredServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            serviceCollection.AddApiClient();

            // Assert - Check that the services are registered
            var apiTokenProviderDescriptor = serviceCollection.SingleOrDefault(sd =>
                sd.ServiceType == typeof(IApiTokenProvider) &&
                sd.ImplementationType == typeof(ApiTokenProvider) &&
                sd.Lifetime == ServiceLifetime.Singleton);
            Assert.NotNull(apiTokenProviderDescriptor);

            var restClientSingletonDescriptor = serviceCollection.SingleOrDefault(sd =>
                sd.ServiceType == typeof(IRestClientSingleton) &&
                sd.ImplementationType == typeof(RestClientSingleton) &&
                sd.Lifetime == ServiceLifetime.Singleton);
            Assert.NotNull(restClientSingletonDescriptor);

            // Check for the new ITokenCredentialProvider registration
            var tokenCredentialProviderDescriptor = serviceCollection.SingleOrDefault(sd =>
                sd.ServiceType == typeof(ITokenCredentialProvider) &&
                sd.ImplementationType == typeof(DefaultTokenCredentialProvider) &&
                sd.Lifetime == ServiceLifetime.Singleton);
            Assert.NotNull(tokenCredentialProviderDescriptor);
        }
    }
}