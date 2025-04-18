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

        [Fact]
        public void AddApiClient_WithCustomTokenCredentialProvider_RegistersCustomImplementation()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Register a mock token credential provider before calling AddApiClient
            serviceCollection.AddSingleton<ITokenCredentialProvider, CustomTokenCredentialProvider>();

            // Act
            serviceCollection.AddApiClient();

            // Assert
            // Verify it didn't replace our custom registration
            var tokenCredentialProviderDescriptor = serviceCollection
                .Where(sd => sd.ServiceType == typeof(ITokenCredentialProvider))
                .First();

            Assert.Equal(typeof(CustomTokenCredentialProvider), tokenCredentialProviderDescriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, tokenCredentialProviderDescriptor.Lifetime);
        }

        [Fact]
        public void AddApiClient_CalledMultipleTimes_RegistersAllRequiredServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            serviceCollection.AddApiClient();
            serviceCollection.AddApiClient(); // Call again

            // Assert
            var apiTokenProviderRegistrations = serviceCollection
                .Where(sd => sd.ServiceType == typeof(IApiTokenProvider))
                .ToList();

            // The implementation currently registers services each time it's called
            Assert.Equal(2, apiTokenProviderRegistrations.Count);

            // Both should be the same implementation type
            Assert.All(apiTokenProviderRegistrations, descriptor =>
                Assert.Equal(typeof(ApiTokenProvider), descriptor.ImplementationType));

            // Verify other required services are also registered
            var restClientSingletonRegistrations = serviceCollection
                .Where(sd => sd.ServiceType == typeof(IRestClientSingleton))
                .ToList();
            Assert.Equal(2, restClientSingletonRegistrations.Count);
        }

        // Simple custom implementation of ITokenCredentialProvider for testing
        private class CustomTokenCredentialProvider : ITokenCredentialProvider
        {
            public Azure.Core.TokenCredential GetTokenCredential()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}