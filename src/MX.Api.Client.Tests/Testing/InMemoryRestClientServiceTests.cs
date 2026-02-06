using System.Net;
using MX.Api.Abstractions;
using MX.Api.Client.Configuration;
using MX.Api.Client.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestSharp;
using Xunit;

namespace MX.Api.Client.Tests.Testing;

/// <summary>
/// Tests demonstrating how to use InMemoryRestClientService for testing
/// </summary>
public class InMemoryRestClientServiceTests
{
    [Fact]
    public void AddResponse_StoresResponseForResource()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        var response = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = "{\"data\": \"test\"}"
        };

        // Act
        service.AddResponse("test/endpoint", response);

        // Assert - Execute and verify the response is returned
        var result = service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Result;
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"data\": \"test\"}", result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_RecordsExecutedRequests()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.AddResponse("test/endpoint", new RestResponse { StatusCode = HttpStatusCode.OK });

        // Act
        await service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get));
        await service.ExecuteAsync("https://test.com", new RestRequest("another/endpoint", Method.Post));

        // Assert
        Assert.Equal(2, service.ExecutedRequests.Count);
        Assert.Equal("test/endpoint", service.ExecutedRequests[0].Resource);
        Assert.Equal("another/endpoint", service.ExecutedRequests[1].Resource);
    }

    [Fact]
    public void WasCalled_ReturnsTrueWhenResourceWasCalled()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.AddResponse("test/endpoint", new RestResponse { StatusCode = HttpStatusCode.OK });

        // Act
        service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Wait();

        // Assert
        Assert.True(service.WasCalled("test/endpoint"));
        Assert.False(service.WasCalled("other/endpoint"));
    }

    [Fact]
    public void WasCalledTimes_CountsRequestsCorrectly()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.AddResponse("test/endpoint", new RestResponse { StatusCode = HttpStatusCode.OK });

        // Act
        service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Wait();
        service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Wait();
        service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Wait();

        // Assert
        Assert.True(service.WasCalledTimes("test/endpoint", 3));
        Assert.False(service.WasCalledTimes("test/endpoint", 2));
    }

    [Fact]
    public async Task AddResponseFunction_AllowsDynamicResponses()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.AddResponseFunction("users", request =>
        {
            var pageParam = request.Parameters.FirstOrDefault(p => p.Name == "page");
            var page = pageParam?.Value?.ToString() ?? "1";

            return new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = $"{{\"page\": {page}}}"
            };
        });

        // Act
        var request = new RestRequest("users", Method.Get);
        request.AddQueryParameter("page", "5");
        var result = await service.ExecuteAsync("https://test.com", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Contains("\"page\": 5", result.Content);
    }

    [Fact]
    public async Task SetDefaultResponse_UsedWhenNoMatchFound()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.SetDefaultResponse(new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = "{\"default\": true}"
        });

        // Act
        var result = await service.ExecuteAsync("https://test.com", new RestRequest("any/endpoint", Method.Get));

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Contains("\"default\": true", result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_Returns404WhenNoResponseConfigured()
    {
        // Arrange
        var service = new InMemoryRestClientService();

        // Act
        var result = await service.ExecuteAsync("https://test.com", new RestRequest("unknown/endpoint", Method.Get));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("No response configured", result.Content);
    }

    [Fact]
    public void Clear_RemovesAllConfiguredResponses()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.AddResponse("test/endpoint", new RestResponse { StatusCode = HttpStatusCode.OK });
        service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Wait();

        // Act
        service.Clear();

        // Assert
        Assert.Empty(service.ExecutedRequests);
        var result = service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Result;
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public void Dispose_DisposesResourcesCleanly()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.AddResponse("test/endpoint", new RestResponse { StatusCode = HttpStatusCode.OK });

        // Act
        service.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() =>
            service.ExecuteAsync("https://test.com", new RestRequest("test/endpoint", Method.Get)).Wait());
    }

    [Fact]
    public async Task ExecuteWithNamedOptionsAsync_WorksSameAsExecuteAsync()
    {
        // Arrange
        var service = new InMemoryRestClientService();
        service.AddResponse("test/endpoint", new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = "{\"test\": true}"
        });

        // Act
        var result = await service.ExecuteWithNamedOptionsAsync("TestOptions", new RestRequest("test/endpoint", Method.Get));

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("{\"test\": true}", result.Content);
    }
}
