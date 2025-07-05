using MX.Api.Client.Configuration;
using Xunit;

namespace MX.Api.Client.Tests;

public class ApiClientOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var options = new ApiClientOptions();

        // Assert
        Assert.Equal(string.Empty, options.BaseUrl);
        Assert.Null(options.AuthenticationOptions);
        Assert.Equal(3, options.MaxRetryCount); // Default retry count is 3
    }

    [Fact]
    public void Create_WithBaseUrl_SetsBaseUrl()
    {
        // Arrange
        var baseUrl = "https://api.example.com";

        // Act
        var options = ApiClientOptions.Create(baseUrl);

        // Assert
        Assert.Equal(baseUrl, options.BaseUrl);
        Assert.Null(options.AuthenticationOptions);
        Assert.Equal(3, options.MaxRetryCount); // Default retry count is 3
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new ApiClientOptions();
        var baseUrl = "https://api.example.com";
        var maxRetryCount = 5;
        var authOptions = new ApiKeyAuthenticationOptions();
        authOptions.SetApiKey("test-key");

        // Act
        options.BaseUrl = baseUrl;
        options.MaxRetryCount = maxRetryCount;
        options.AuthenticationOptions = authOptions;

        // Assert
        Assert.Equal(baseUrl, options.BaseUrl);
        Assert.Equal(maxRetryCount, options.MaxRetryCount);
        Assert.Same(authOptions, options.AuthenticationOptions);
    }

    [Fact]
    public void WithBaseUrl_SetsBaseUrl_AndReturnsInstance()
    {
        // Arrange
        var options = new ApiClientOptions();
        var baseUrl = "https://api.example.com";

        // Act
        var result = options.WithBaseUrl(baseUrl);

        // Assert
        Assert.Equal(baseUrl, options.BaseUrl);
        Assert.Same(options, result); // Returns the same instance for fluent chaining
    }

    [Fact]
    public void WithMaxRetryCount_SetsMaxRetryCount_AndReturnsInstance()
    {
        // Arrange
        var options = new ApiClientOptions();
        var maxRetryCount = 5;

        // Act
        var result = options.WithMaxRetryCount(maxRetryCount);

        // Assert
        Assert.Equal(maxRetryCount, options.MaxRetryCount);
        Assert.Same(options, result); // Returns the same instance for fluent chaining
    }

    [Fact]
    public void WithAuthentication_SetsAuthenticationOptions_AndReturnsInstance()
    {
        // Arrange
        var options = new ApiClientOptions();
        var authOptions = new ApiKeyAuthenticationOptions();
        authOptions.SetApiKey("test-key");

        // Act
        var result = options.WithAuthentication(authOptions);

        // Assert
        Assert.Same(authOptions, options.AuthenticationOptions);
        Assert.Same(options, result); // Returns the same instance for fluent chaining

        // Clean up
        authOptions.Dispose();
    }

    [Fact]
    public void WithApiKeyAuthentication_SetsApiKeyAuthenticationOptions_AndReturnsInstance()
    {
        // Arrange
        var options = new ApiClientOptions();
        var apiKey = "test-key";
        var headerName = "Custom-Api-Key";

        // Act
        var result = options.WithApiKeyAuthentication(apiKey, headerName);

        // Assert
        Assert.NotNull(options.AuthenticationOptions);
        Assert.IsType<ApiKeyAuthenticationOptions>(options.AuthenticationOptions);
        var apiKeyOptions = options.AuthenticationOptions as ApiKeyAuthenticationOptions;
        Assert.Equal(apiKey, apiKeyOptions?.GetApiKeyAsString());
        Assert.Equal(headerName, apiKeyOptions?.HeaderName);
        Assert.Same(options, result); // Returns the same instance for fluent chaining
    }

    [Fact]
    public void FluentApiChaining_ConfiguresAllProperties()
    {
        // Arrange
        var baseUrl = "https://api.example.com";
        var maxRetryCount = 5;
        var apiKey = "test-key";

        // Act
        var options = new ApiClientOptions()
            .WithBaseUrl(baseUrl)
            .WithMaxRetryCount(maxRetryCount)
            .WithApiKeyAuthentication(apiKey);

        // Assert
        Assert.Equal(baseUrl, options.BaseUrl);
        Assert.Equal(maxRetryCount, options.MaxRetryCount);
        Assert.IsType<ApiKeyAuthenticationOptions>(options.AuthenticationOptions);
        var apiKeyOptions = options.AuthenticationOptions as ApiKeyAuthenticationOptions;
        Assert.Equal(apiKey, apiKeyOptions?.GetApiKeyAsString());
    }
}
