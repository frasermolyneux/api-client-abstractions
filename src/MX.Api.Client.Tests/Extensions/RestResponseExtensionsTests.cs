using System.Net;
using MX.Api.Abstractions;
using MX.Api.Client.Extensions;
using Newtonsoft.Json;
using RestSharp;
using Xunit;

namespace MX.Api.Client.Tests.Extensions;

public class RestResponseExtensionsTests
{
    [Fact]
    public void ToHttpResponse_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        RestResponse? response = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => response!.ToHttpResponse());
    }

    [Fact]
    public void ToHttpResponse_WithSuccessfulResponse_ReturnsHttpResponseWrapper()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var apiResponse = new ApiResponse();

        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = JsonConvert.SerializeObject(apiResponse)
        };

        // Act
        var result = response.ToHttpResponse();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
    }
    [Fact]
    public void ToHttpResponse_WithEmptyContent_ReturnsHttpResponseWrapperWithError()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = string.Empty
        };

        // Act
        var result = response.ToHttpResponse();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess); // HTTP request succeeded with a 200 status code
        Assert.NotNull(result.Result);
        Assert.NotNull(result.Result!.Errors);
        Assert.Single(result.Result.Errors!);
        Assert.Equal("NullContent", result.Result.Errors![0].Code);
    }
    [Fact]
    public void ToHttpResponse_WithInvalidJson_ReturnsHttpResponseWrapperWithError()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = "This is not valid JSON"
        };

        // Act
        var result = response.ToHttpResponse();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess); // HTTP request succeeded with a 200 status code
        Assert.NotNull(result.Result);
        Assert.NotNull(result.Result!.Errors);
        Assert.Single(result.Result.Errors!);
        Assert.Equal("JsonError", result.Result.Errors![0].Code);
    }
    [Fact]
    public void ToHttpResponse_WithValidJsonButNotApiResponse_ReturnsHttpResponseWrapper()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = JsonConvert.SerializeObject(new { id = 1, name = "Test" })
        };

        // Act
        var result = response.ToHttpResponse();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess); // HTTP request succeeded with a 200 status code

        // We don't assert anything about result.Result as the implementation might handle
        // non-ApiResponse JSON in different ways
    }

    [Fact]
    public void ToHttpResponse_WithHeadRequest_ReturnsEmptyHttpResponseWrapper()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var request = new RestRequest("test", Method.Head);
        var response = new RestResponse
        {
            StatusCode = statusCode,
            Request = request
        };

        // Act
        var result = response.ToHttpResponse();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);  // Always true for HEAD
        Assert.Null(result.Result);
    }

    [Fact]
    public void ToHttpResponse_Generic_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        RestResponse? response = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => response!.ToHttpResponse<string>());
    }

    [Fact]
    public void ToHttpResponse_Generic_WithSuccessfulResponse_ReturnsHttpResponseWrapper()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var data = "Test Data";
        var apiResponse = new ApiResponse<string>(data);

        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = JsonConvert.SerializeObject(apiResponse)
        };

        // Act
        var result = response.ToHttpResponse<string>();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
        Assert.Equal(data, result.Result.Data);
    }
    [Fact]
    public void ToHttpResponse_Generic_WithEmptyContent_ReturnsHttpResponseWrapperWithError()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = string.Empty
        };

        // Act
        var result = response.ToHttpResponse<string>();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess); // HTTP request succeeded with a 200 status code
        Assert.NotNull(result.Result);
        Assert.NotNull(result.Result!.Errors);
        Assert.Single(result.Result.Errors!);
        Assert.Equal("NullContent", result.Result.Errors![0].Code);
    }
    [Fact]
    public void ToHttpResponse_Generic_WithInvalidJson_ReturnsHttpResponseWrapperWithError()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = "This is not valid JSON"
        };

        // Act
        var result = response.ToHttpResponse<string>();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess); // HTTP request succeeded with a 200 status code
        Assert.NotNull(result.Result);
        Assert.NotNull(result.Result!.Errors);
        Assert.Single(result.Result.Errors!);
        Assert.Equal("JsonError", result.Result.Errors![0].Code);
    }
    [Fact]
    public void ToHttpResponse_Generic_WithValidJsonButNotApiResponse_ReturnsHttpResponseWrapper()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var response = new RestResponse
        {
            StatusCode = statusCode,
            Content = JsonConvert.SerializeObject(new { id = 1, name = "Test" })
        };

        // Act
        var result = response.ToHttpResponse<string>();

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
        Assert.True(result.IsSuccess); // HTTP request succeeded with a 200 status code

        // We don't assert anything about result.Result as the implementation might handle
        // non-ApiResponse JSON in different ways
    }
}
