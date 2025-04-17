using Azure.Core;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MxIO.ApiClient
{
    public class ApiTokenProviderTests : IDisposable
    {
        private readonly Mock<ILogger<ApiTokenProvider>> loggerMock;
        private readonly TestMemoryCache memoryCache;
        private readonly Mock<ITokenCredentialProvider> tokenCredentialProviderMock;
        private readonly Mock<TokenCredential> tokenCredentialMock;
        private readonly ApiTokenProvider apiTokenProvider;

        public ApiTokenProviderTests()
        {
            loggerMock = new Mock<ILogger<ApiTokenProvider>>();
            memoryCache = new TestMemoryCache();

            // Create a mock token credential provider and token credential
            tokenCredentialProviderMock = new Mock<ITokenCredentialProvider>();
            tokenCredentialMock = new Mock<TokenCredential>();

            tokenCredentialProviderMock.Setup(tcp => tcp.GetTokenCredential())
                .Returns(tokenCredentialMock.Object);

            apiTokenProvider = new ApiTokenProvider(loggerMock.Object, memoryCache, tokenCredentialProviderMock.Object);
        }

        public void Dispose()
        {
            memoryCache.Dispose();
        }

        [Fact]
        public void ApiTokenProvider_Constructor_InitializesCorrectly()
        {
            // Act & Assert - Just verify that the constructor doesn't throw exceptions
            var instance = new ApiTokenProvider(loggerMock.Object, memoryCache, tokenCredentialProviderMock.Object);
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task GetAccessToken_WhenNotCached_AcquiresNewToken()
        {
            // Arrange
            var audience = "test-audience";
            var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
            var mockToken = new AccessToken("mock-token-value", expiresOn);

            tokenCredentialMock.Setup(tc => tc.GetTokenAsync(
                    It.Is<TokenRequestContext>(c => c.Scopes[0] == $"{audience}/.default"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockToken);

            // Act
            var result = await apiTokenProvider.GetAccessToken(audience);

            // Assert
            Assert.Equal("mock-token-value", result);

            // Verify token credential provider was called
            tokenCredentialProviderMock.Verify(tcp => tcp.GetTokenCredential(), Times.Once);

            // Verify GetTokenAsync was called with the correct scope
            tokenCredentialMock.Verify(tc => tc.GetTokenAsync(
                    It.Is<TokenRequestContext>(c => c.Scopes[0] == $"{audience}/.default"),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify token was cached
            Assert.True(memoryCache.TryGetValue(audience, out object? cachedValue));
            Assert.NotNull(cachedValue);
            Assert.Equal("mock-token-value", ((AccessToken)cachedValue!).Token);
        }

        [Fact]
        public async Task GetAccessToken_WhenCached_ReturnsFromCache()
        {
            // Arrange
            var audience = "test-audience";
            var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
            var token = new AccessToken("cached-token-value", expiresOn);

            // Pre-populate the cache
            memoryCache.Set(audience, token);

            // Act
            var result = await apiTokenProvider.GetAccessToken(audience);

            // Assert
            Assert.Equal("cached-token-value", result);

            // Verify token credential provider was NOT called
            tokenCredentialProviderMock.Verify(tcp => tcp.GetTokenCredential(), Times.Never);

            // Verify GetTokenAsync was NOT called
            tokenCredentialMock.Verify(tc => tc.GetTokenAsync(
                It.IsAny<TokenRequestContext>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        }

        [Fact]
        public async Task GetAccessToken_WhenCacheExpired_GetsNewToken()
        {
            // Arrange
            var audience = "test-audience";
            var expiredOn = DateTimeOffset.UtcNow.AddHours(-1);
            var expiredToken = new AccessToken("expired-token", expiredOn);

            // Pre-populate the cache with an expired token
            memoryCache.Set(audience, expiredToken);

            // Setup new token to be returned by the credential
            var newToken = new AccessToken("new-token", DateTimeOffset.UtcNow.AddHours(1));
            tokenCredentialMock.Setup(tc => tc.GetTokenAsync(
                    It.Is<TokenRequestContext>(c => c.Scopes[0] == $"{audience}/.default"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newToken);

            // Act
            var result = await apiTokenProvider.GetAccessToken(audience);

            // Assert
            Assert.Equal("new-token", result);

            // Verify token credential provider was called
            tokenCredentialProviderMock.Verify(tcp => tcp.GetTokenCredential(), Times.Once);

            // Verify the new token was cached
            Assert.True(memoryCache.TryGetValue(audience, out object? cachedValue));
            Assert.NotNull(cachedValue);
            Assert.Equal("new-token", ((AccessToken)cachedValue!).Token);
        }

        [Fact]
        public async Task GetAccessToken_WhenTokenAcquisitionFails_LogsErrorAndRethrows()
        {
            // Arrange
            var audience = "test-audience";
            var expectedException = new Exception("Authentication failed");

            tokenCredentialMock.Setup(tc => tc.GetTokenAsync(
                    It.IsAny<TokenRequestContext>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => apiTokenProvider.GetAccessToken(audience));
            Assert.Equal("Authentication failed", exception.Message);
        }
    }
}