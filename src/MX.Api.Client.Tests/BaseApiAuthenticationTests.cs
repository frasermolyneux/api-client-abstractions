using Microsoft.Extensions.Logging;
using Moq;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Client.Tests.TestClients;
using RestSharp;
using Xunit;

namespace MX.Api.Client.Tests;

public class BaseApiAuthenticationTests
{
    private readonly Mock<ILogger<BaseApi<TestApiOptions>>> _loggerMock = new();
    private readonly Mock<IRestClientService> _restClientServiceMock = new();

    private TestApiClient CreateClient(TestApiOptions options)
    {
        return new TestApiClient(
            _loggerMock.Object,
            null,
            _restClientServiceMock.Object,
            options);
    }

    [Fact]
    public async Task CreateRequestAsync_WithApiKeyHeader_AddsHeader()
    {
        // Arrange
        var options = new TestApiOptions { BaseUrl = "https://api.example.com" };
        var apiKeyAuth = new ApiKeyAuthenticationOptions
        {
            HeaderName = "X-Api-Key",
            Location = ApiKeyLocation.Header
        };
        apiKeyAuth.SetApiKey("test-key-123");
        options.AuthenticationOptions.Add(apiKeyAuth);

        var client = CreateClient(options);

        // Act
        var request = await client.CreateRequestAsync("test/resource", Method.Get);

        // Assert
        var header = request.Parameters.FirstOrDefault(p =>
            p.Type == ParameterType.HttpHeader && p.Name == "X-Api-Key");
        Assert.NotNull(header);
        Assert.Equal("test-key-123", header.Value);

        // Ensure it was NOT added as a query parameter
        var queryParam = request.Parameters.FirstOrDefault(p =>
            p.Type == ParameterType.QueryString && p.Name == "X-Api-Key");
        Assert.Null(queryParam);
    }

    [Fact]
    public async Task CreateRequestAsync_WithApiKeyQueryParameter_AddsQueryParameter()
    {
        // Arrange
        var options = new TestApiOptions { BaseUrl = "https://api.example.com" };
        var apiKeyAuth = new ApiKeyAuthenticationOptions
        {
            HeaderName = "key",
            Location = ApiKeyLocation.QueryParameter
        };
        apiKeyAuth.SetApiKey("test-key-456");
        options.AuthenticationOptions.Add(apiKeyAuth);

        var client = CreateClient(options);

        // Act
        var request = await client.CreateRequestAsync("test/resource", Method.Get);

        // Assert
        var queryParam = request.Parameters.FirstOrDefault(p =>
            p.Type == ParameterType.QueryString && p.Name == "key");
        Assert.NotNull(queryParam);
        Assert.Equal("test-key-456", queryParam.Value);

        // Ensure it was NOT added as a header
        var header = request.Parameters.FirstOrDefault(p =>
            p.Type == ParameterType.HttpHeader && p.Name == "key");
        Assert.Null(header);
    }

    [Fact]
    public async Task CreateRequestAsync_WithDefaultApiKeyLocation_AddsAsHeader()
    {
        // Arrange - use default Location (should be Header)
        var options = new TestApiOptions { BaseUrl = "https://api.example.com" };
        var apiKeyAuth = new ApiKeyAuthenticationOptions();
        apiKeyAuth.SetApiKey("default-key");
        options.AuthenticationOptions.Add(apiKeyAuth);

        var client = CreateClient(options);

        // Act
        var request = await client.CreateRequestAsync("test/resource", Method.Get);

        // Assert - default header name is "Ocp-Apim-Subscription-Key"
        var header = request.Parameters.FirstOrDefault(p =>
            p.Type == ParameterType.HttpHeader && p.Name == "Ocp-Apim-Subscription-Key");
        Assert.NotNull(header);
        Assert.Equal("default-key", header.Value);
    }

    [Fact]
    public async Task CreateRequestAsync_WithNoAuthentication_NoAuthParametersAdded()
    {
        // Arrange
        var options = new TestApiOptions { BaseUrl = "https://api.example.com" };
        var client = CreateClient(options);

        // Act
        var request = await client.CreateRequestAsync("test/resource", Method.Get);

        // Assert
        var authParams = request.Parameters.Where(p =>
            p.Type == ParameterType.HttpHeader || p.Type == ParameterType.QueryString).ToList();
        Assert.Empty(authParams);
    }

    [Fact]
    public async Task CreateRequestAsync_WithEmptyApiKey_NoParametersAdded()
    {
        // Arrange
        var options = new TestApiOptions { BaseUrl = "https://api.example.com" };
        var apiKeyAuth = new ApiKeyAuthenticationOptions
        {
            HeaderName = "key",
            Location = ApiKeyLocation.QueryParameter
        };
        // Don't set API key - leave it empty
        options.AuthenticationOptions.Add(apiKeyAuth);

        var client = CreateClient(options);

        // Act
        var request = await client.CreateRequestAsync("test/resource", Method.Get);

        // Assert - no query parameter should be added
        var queryParam = request.Parameters.FirstOrDefault(p =>
            p.Type == ParameterType.QueryString && p.Name == "key");
        Assert.Null(queryParam);
    }
}
