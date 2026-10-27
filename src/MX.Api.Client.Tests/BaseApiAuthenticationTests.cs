using Microsoft.Extensions.Logging;
using Moq;
using MX.Api.Client.Configuration;
using MX.Api.Client.Tests.TestClients;
using RestSharp;
using Xunit;

namespace MX.Api.Client.Tests;

public class BaseApiAuthenticationTests
{
    [Theory]
    [InlineData(System.Net.HttpStatusCode.BadRequest)]
    [InlineData(System.Net.HttpStatusCode.UnprocessableEntity)]
    [InlineData(System.Net.HttpStatusCode.Unauthorized)]
    public async Task ExecuteAsync_HttpFailureRetainsStructuredErrorsAndStatus(System.Net.HttpStatusCode status)
    {
        var client = CreateClient(new TestApiOptions { BaseUrl = "https://api.example.com" });
        var response = new RestResponse
        {
            StatusCode = status,
            Content = Newtonsoft.Json.JsonConvert.SerializeObject(new Abstractions.ApiResponse(
                new Abstractions.ApiError("GAME_TYPE_MISMATCH", "Choose a compatible server.", "GameServerId")))
        };
        _ = _restClientServiceMock.Setup(x => x.ExecuteAsync(
            It.IsAny<string>(), It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var failure = await Assert.ThrowsAsync<ApiHttpRequestException>(() =>
            client.ExecuteAsync(new RestRequest("assignments", Method.Post)));

        _ = Assert.IsAssignableFrom<HttpRequestException>(failure);
        Assert.Equal(status, failure.StatusCode);
        var error = Assert.Single(failure.ErrorResponse!.Errors!);
        Assert.Equal("GAME_TYPE_MISMATCH", error.Code);
        Assert.Equal("GameServerId", error.Target);
        _restClientServiceMock.Verify(x => x.ExecuteAsync(
            It.IsAny<string>(), It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NonJsonFailureStillThrowsWithActualStatus()
    {
        var client = CreateClient(new TestApiOptions { BaseUrl = "https://api.example.com" });
        _ = _restClientServiceMock.Setup(x => x.ExecuteAsync(
            It.IsAny<string>(), It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Content = "<html>Invalid request</html>" });

        var failure = await Assert.ThrowsAsync<ApiHttpRequestException>(() =>
            client.ExecuteAsync(new RestRequest("assignments", Method.Post)));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, failure.StatusCode);
    }

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
            p.Type is ParameterType.HttpHeader or ParameterType.QueryString).ToList();
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
