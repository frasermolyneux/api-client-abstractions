using MX.Api.Client.Testing;
using Xunit;

namespace MX.Api.Client.Tests.Testing;

/// <summary>
/// Tests demonstrating how to use FakeApiTokenProvider for testing
/// </summary>
public class FakeApiTokenProviderTests
{
    [Fact]
    public async Task GetAccessTokenAsync_ReturnsConfiguredToken()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://test", "test-token-123");

        // Act
        var token = await provider.GetAccessTokenAsync("api://test");

        // Assert
        Assert.Equal("test-token-123", token);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsDefaultTokenWhenNotConfigured()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetDefaultToken("default-token");

        // Act
        var token = await provider.GetAccessTokenAsync("api://unknown");

        // Assert
        Assert.Equal("default-token", token);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsGenericTokenWhenNoDefaultSet()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();

        // Act
        var token = await provider.GetAccessTokenAsync("api://unknown");

        // Assert
        Assert.Equal("fake-test-token", token);
    }

    [Fact]
    public async Task GetAccessTokenAsync_RecordsTokenRequests()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://test", "test-token");

        // Act
        await provider.GetAccessTokenAsync("api://test");
        await provider.GetAccessTokenAsync("api://other");

        // Assert
        Assert.Equal(2, provider.TokenRequests.Count);
        Assert.Equal("api://test", provider.TokenRequests[0].Audience);
        Assert.Equal("api://other", provider.TokenRequests[1].Audience);
    }

    [Fact]
    public async Task WasTokenRequested_ReturnsTrueWhenRequested()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://test", "test-token");

        // Act
        await provider.GetAccessTokenAsync("api://test");

        // Assert
        Assert.True(provider.WasTokenRequested("api://test"));
        Assert.False(provider.WasTokenRequested("api://other"));
    }

    [Fact]
    public async Task WasTokenRequestedTimes_CountsRequestsCorrectly()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://test", "test-token");

        // Act
        await provider.GetAccessTokenAsync("api://test");
        await provider.GetAccessTokenAsync("api://test");
        await provider.GetAccessTokenAsync("api://test");

        // Assert
        Assert.True(provider.WasTokenRequestedTimes("api://test", 3));
        Assert.False(provider.WasTokenRequestedTimes("api://test", 2));
    }

    [Fact]
    public async Task SetToken_SupportsCaseInsensitiveAudience()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://TEST", "test-token");

        // Act
        var token = await provider.GetAccessTokenAsync("api://test");

        // Assert
        Assert.Equal("test-token", token);
    }

    [Fact]
    public void Clear_RemovesAllConfiguredTokens()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://test", "test-token");
        provider.SetDefaultToken("default-token");
        provider.GetAccessTokenAsync("api://test").Wait();

        // Act
        provider.Clear();

        // Assert
        Assert.Empty(provider.TokenRequests);
        var token = provider.GetAccessTokenAsync("api://test").Result;
        Assert.Equal("fake-test-token", token); // Falls back to generic token
    }

    [Fact]
    public async Task SetToken_OverwritesPreviousToken()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://test", "old-token");

        // Act
        provider.SetToken("api://test", "new-token");
        var token = await provider.GetAccessTokenAsync("api://test");

        // Assert
        Assert.Equal("new-token", token);
    }

    [Fact]
    public async Task GetAccessTokenAsync_WorksWithMultipleAudiences()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();
        provider.SetToken("api://users", "users-token");
        provider.SetToken("api://orders", "orders-token");
        provider.SetToken("api://products", "products-token");

        // Act
        var usersToken = await provider.GetAccessTokenAsync("api://users");
        var ordersToken = await provider.GetAccessTokenAsync("api://orders");
        var productsToken = await provider.GetAccessTokenAsync("api://products");

        // Assert
        Assert.Equal("users-token", usersToken);
        Assert.Equal("orders-token", ordersToken);
        Assert.Equal("products-token", productsToken);
    }

    [Fact]
    public void SetToken_ThrowsWhenAudienceIsNull()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.SetToken(null!, "token"));
    }

    [Fact]
    public void SetToken_ThrowsWhenTokenIsNull()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.SetToken("api://test", null!));
    }

    [Fact]
    public void SetDefaultToken_ThrowsWhenTokenIsNull()
    {
        // Arrange
        var provider = new FakeApiTokenProvider();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.SetDefaultToken(null!));
    }
}
