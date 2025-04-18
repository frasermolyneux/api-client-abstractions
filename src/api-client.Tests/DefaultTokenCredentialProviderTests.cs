using Azure.Core;
using Moq;
using System;
using Xunit;

namespace MxIO.ApiClient
{
    public class DefaultTokenCredentialProviderTests
    {
        [Fact]
        public void GetTokenCredential_ReturnsDefaultAzureCredential()
        {
            // Arrange
            var provider = new DefaultTokenCredentialProvider();

            // Act
            var credential = provider.GetTokenCredential();

            // Assert
            Assert.NotNull(credential);
        }
    }
}