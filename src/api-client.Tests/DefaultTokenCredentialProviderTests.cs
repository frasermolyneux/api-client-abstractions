using Azure.Core;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MxIO.ApiClient
{
    public class DefaultTokenCredentialProviderTests
    {
        [Fact]
        public async Task GetTokenCredentialAsync_ReturnsDefaultAzureCredential()
        {
            // Arrange
            var provider = new DefaultTokenCredentialProvider();
            var cancellationToken = CancellationToken.None;

            // Act
            var credential = await provider.GetTokenCredentialAsync(cancellationToken);

            // Assert
            Assert.NotNull(credential);
        }
    }
}