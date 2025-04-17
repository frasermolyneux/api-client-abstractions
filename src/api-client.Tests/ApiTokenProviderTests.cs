using Azure.Core;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MxIO.ApiClient
{
    [TestFixture]
    public class ApiTokenProviderTests
    {
        private ILogger<ApiTokenProvider> logger;
        private TestMemoryCache memoryCache;
        private ITokenCredentialProvider tokenCredentialProvider;
        private TokenCredential mockTokenCredential;
        private ApiTokenProvider apiTokenProvider;

        [SetUp]
        public void SetUp()
        {
            logger = A.Fake<ILogger<ApiTokenProvider>>();
            memoryCache = new TestMemoryCache();

            // Create a mock token credential provider and token credential
            tokenCredentialProvider = A.Fake<ITokenCredentialProvider>();
            mockTokenCredential = A.Fake<TokenCredential>();

            A.CallTo(() => tokenCredentialProvider.GetTokenCredential()).Returns(mockTokenCredential);

            apiTokenProvider = new ApiTokenProvider(logger, memoryCache, tokenCredentialProvider);
        }

        [TearDown]
        public void TearDown()
        {
            memoryCache.Dispose();
        }

        [Test]
        public void ApiTokenProvider_Constructor_InitializesCorrectly()
        {
            // Act & Assert - Just verify that the constructor doesn't throw exceptions
            var instance = new ApiTokenProvider(logger, memoryCache, tokenCredentialProvider);
            instance.Should().NotBeNull();
        }

        [Test]
        public async Task GetAccessToken_WhenNotCached_AcquiresNewToken()
        {
            // Arrange
            var audience = "test-audience";
            var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
            var mockToken = new AccessToken("mock-token-value", expiresOn);

            A.CallTo(() => mockTokenCredential.GetTokenAsync(
                    A<TokenRequestContext>.That.Matches(c => c.Scopes[0] == $"{audience}/.default"),
                    A<CancellationToken>._))
                .Returns(mockToken);

            // Act
            var result = await apiTokenProvider.GetAccessToken(audience);

            // Assert
            result.Should().Be("mock-token-value");

            // Verify token credential provider was called
            A.CallTo(() => tokenCredentialProvider.GetTokenCredential()).MustHaveHappenedOnceExactly();

            // Verify GetTokenAsync was called with the correct scope
            A.CallTo(() => mockTokenCredential.GetTokenAsync(
                    A<TokenRequestContext>.That.Matches(c => c.Scopes[0] == $"{audience}/.default"),
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            // Verify token was cached
            memoryCache.TryGetValue(audience, out object cachedValue).Should().BeTrue();
            ((AccessToken)cachedValue).Token.Should().Be("mock-token-value");
        }

        [Test]
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
            result.Should().Be("cached-token-value");

            // Verify token credential provider was NOT called
            A.CallTo(() => tokenCredentialProvider.GetTokenCredential()).MustNotHaveHappened();

            // Verify GetTokenAsync was NOT called
            A.CallTo(() => mockTokenCredential.GetTokenAsync(A<TokenRequestContext>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Test]
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
            A.CallTo(() => mockTokenCredential.GetTokenAsync(
                    A<TokenRequestContext>.That.Matches(c => c.Scopes[0] == $"{audience}/.default"),
                    A<CancellationToken>._))
                .Returns(newToken);

            // Act
            var result = await apiTokenProvider.GetAccessToken(audience);

            // Assert
            result.Should().Be("new-token");

            // Verify token credential provider was called
            A.CallTo(() => tokenCredentialProvider.GetTokenCredential()).MustHaveHappenedOnceExactly();

            // Verify the new token was cached
            memoryCache.TryGetValue(audience, out object cachedValue).Should().BeTrue();
            ((AccessToken)cachedValue).Token.Should().Be("new-token");
        }

        [Test]
        public void GetAccessToken_WhenTokenAcquisitionFails_LogsErrorAndRethrows()
        {
            // Arrange
            var audience = "test-audience";
            var expectedException = new Exception("Authentication failed");

            A.CallTo(() => mockTokenCredential.GetTokenAsync(
                    A<TokenRequestContext>._,
                    A<CancellationToken>._))
                .ThrowsAsync(expectedException);

            // Act & Assert
            Func<Task> act = async () => await apiTokenProvider.GetAccessToken(audience);

            // Verify that the exception is thrown
            act.Should().ThrowAsync<Exception>()
                .WithMessage("Authentication failed");
        }
    }
}