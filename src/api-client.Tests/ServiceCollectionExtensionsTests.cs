using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MxIO.ApiClient.Extensions;
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
            serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(IApiTokenProvider) &&
                                               sd.ImplementationType == typeof(ApiTokenProvider) &&
                                               sd.Lifetime == ServiceLifetime.Singleton);

            serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(IRestClientSingleton) &&
                                               sd.ImplementationType == typeof(RestClientSingleton) &&
                                               sd.Lifetime == ServiceLifetime.Singleton);

            // Check for the new ITokenCredentialProvider registration
            serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(ITokenCredentialProvider) &&
                                               sd.ImplementationType == typeof(DefaultTokenCredentialProvider) &&
                                               sd.Lifetime == ServiceLifetime.Singleton);
        }
    }
}