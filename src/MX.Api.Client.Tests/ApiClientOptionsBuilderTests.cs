using MX.Api.Client.Configuration;
using MX.Api.Client.Tests.TestClients;
using Xunit;

namespace MX.Api.Client.Tests;

public class ApiClientOptionsBuilderTests
{
    [Fact]
    public void WithApiKeyAuthentication_DefaultLocation_SetsHeaderLocation()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("test-key");
        var options = builder.Build();

        // Assert
        var apiKeyOptions = Assert.Single(options.AuthenticationOptions);
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(apiKeyOptions);
        Assert.Equal("test-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("Ocp-Apim-Subscription-Key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_QueryParameterLocation_SetsCorrectly()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("my-api-key", "key", ApiKeyLocation.QueryParameter);
        var options = builder.Build();

        // Assert
        var apiKeyOptions = Assert.Single(options.AuthenticationOptions);
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(apiKeyOptions);
        Assert.Equal("my-api-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.QueryParameter, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_CustomHeaderName_SetsCorrectly()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("test-key", "X-Custom-Key");
        var options = builder.Build();

        // Assert
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(Assert.Single(options.AuthenticationOptions));
        Assert.Equal("test-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("X-Custom-Key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_ExplicitHeaderLocation_SetsCorrectly()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        builder.WithBaseUrl("https://api.example.com")
               .WithApiKeyAuthentication("test-key", "X-Api-Key", ApiKeyLocation.Header);
        var options = builder.Build();

        // Assert
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(Assert.Single(options.AuthenticationOptions));
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
        Assert.Equal("X-Api-Key", apiKeyAuth.HeaderName);
    }

    [Fact]
    public void WithSubscriptionKey_DefaultLocation_SetsHeaderLocation()
    {
        // Arrange & Act
        var builder = new TestApiOptionsBuilder();
        builder.WithBaseUrl("https://api.example.com")
               .WithSubscriptionKey("sub-key");
        var options = builder.Build();

        // Assert
        var apiKeyAuth = Assert.IsType<ApiKeyAuthenticationOptions>(Assert.Single(options.AuthenticationOptions));
        Assert.Equal("sub-key", apiKeyAuth.GetApiKeyAsString());
        Assert.Equal("Ocp-Apim-Subscription-Key", apiKeyAuth.HeaderName);
        Assert.Equal(ApiKeyLocation.Header, apiKeyAuth.Location);
    }

    [Fact]
    public void WithApiKeyAuthentication_NullApiKey_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();
        Assert.Throws<ArgumentNullException>(() =>
            builder.WithApiKeyAuthentication(null!));
    }

    [Fact]
    public void WithApiKeyAuthentication_EmptyApiKey_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.WithApiKeyAuthentication(""));
    }

    [Fact]
    public void WithApiKeyAuthentication_EmptyHeaderName_ThrowsArgumentException()
    {
        var builder = new TestApiOptionsBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.WithApiKeyAuthentication("test-key", ""));
    }
}
