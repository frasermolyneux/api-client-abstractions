using System.Net;
using Microsoft.AspNetCore.Mvc;
using MxIO.ApiClient.Abstractions;
using MxIO.ApiClient.WebExtensions;
using Xunit;

namespace MxIO.ApiClient.Abstractions.Tests;

public class ApiResponseDtoExtensionsTests
{
    [Fact]
    public void ToHttpResult_Generic_ShouldReturnObjectResultWithCorrectStatusCode()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.OK, "Test Result");

        // Act
        var result = apiResponse.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
        Assert.Same(apiResponse, objectResult.Value);
    }

    [Fact]
    public void ToHttpResult_Generic_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        ApiResponseDto<string>? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToHttpResult());
    }

    [Fact]
    public void ToHttpResult_NonGeneric_ShouldReturnObjectResultWithCorrectStatusCode()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.BadRequest, "Error message");

        // Act
        var result = apiResponse.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        Assert.Same(apiResponse, objectResult.Value);
    }

    [Fact]
    public void ToHttpResult_NonGeneric_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        ApiResponseDto? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToHttpResult());
    }

    [Fact]
    public void CreateResponse_FromStatusCode_ShouldCreateApiResponseWithCorrectStatusCode()
    {
        // Arrange
        var statusCode = HttpStatusCode.Created;

        // Act
        var response = statusCode.CreateResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void CreateResponse_FromStatusCodeWithResult_ShouldCreateApiResponseWithCorrectStatusCodeAndResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var result = "Test Result";

        // Act
        var response = statusCode.CreateResponse(result);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Empty(response.Errors);
        Assert.Equal(result, response.Result);
    }

    [Fact]
    public void CreateResponse_FromStatusCodeWithNullResult_ShouldCreateApiResponseWithCorrectStatusCodeAndNullResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        string? result = null;

        // Act
        var response = statusCode.CreateResponse(result);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Empty(response.Errors);
        Assert.Null(response.Result);
    }
}