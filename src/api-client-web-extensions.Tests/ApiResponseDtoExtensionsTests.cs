using System.Net;

using Microsoft.AspNetCore.Mvc;

using MxIO.ApiClient.Abstractions;
using MxIO.ApiClient.WebExtensions;

using Xunit;

namespace MxIO.ApiClient.WebExtensions.Tests;

public class ApiResponseDtoExtensionsTests
{
    [Fact]
    public void ToHttpResult_Generic_NullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        IApiResponseDto<string>? apiResponseDto = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => apiResponseDto!.ToHttpResult());
        Assert.Equal("apiResponseDto", exception.ParamName);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public void ToHttpResult_Generic_ReturnsObjectResultWithCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var apiResponseDto = new ApiResponseDto<string>(statusCode, "Test Result");

        // Act
        var result = apiResponseDto.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)statusCode, objectResult.StatusCode);
        Assert.Same(apiResponseDto, objectResult.Value);
    }

    [Fact]
    public void ToHttpResult_Generic_ReturnsObjectResultWithResponseContent()
    {
        // Arrange
        var apiResponseDto = new ApiResponseDto<string>(HttpStatusCode.OK, "Test Result");

        // Act
        var result = apiResponseDto.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var responseDto = Assert.IsType<ApiResponseDto<string>>(objectResult.Value);
        Assert.Equal("Test Result", responseDto.Result);
        Assert.Equal(HttpStatusCode.OK, responseDto.StatusCode);
    }

    [Fact]
    public void ToHttpResult_NullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        IApiResponseDto? apiResponseDto = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => apiResponseDto!.ToHttpResult());
        Assert.Equal("apiResponseDto", exception.ParamName);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public void ToHttpResult_ReturnsObjectResultWithCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var apiResponseDto = new ApiResponseDto(statusCode);

        // Act
        var result = apiResponseDto.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)statusCode, objectResult.StatusCode);
        Assert.Same(apiResponseDto, objectResult.Value);
    }

    [Fact]
    public void ToHttpResult_ReturnsObjectResultWithResponseContent()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };
        var apiResponseDto = new ApiResponseDto(HttpStatusCode.BadRequest, errors);

        // Act
        var result = apiResponseDto.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var responseDto = Assert.IsType<ApiResponseDto>(objectResult.Value);
        Assert.Equal(HttpStatusCode.BadRequest, responseDto.StatusCode);
        Assert.Collection(responseDto.Errors,
            error => Assert.Equal("Error 1", error),
            error => Assert.Equal("Error 2", error));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public void CreateResponse_ReturnsApiResponseDtoWithCorrectStatusCode(HttpStatusCode statusCode)
    {
        // Act
        var response = statusCode.CreateResponse();

        // Assert
        Assert.IsType<ApiResponseDto>(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Empty(response.Errors);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public void CreateResponse_Generic_ReturnsApiResponseDtoWithCorrectStatusCodeAndResult(HttpStatusCode statusCode)
    {
        // Arrange
        string result = "Test Result";

        // Act
        var response = statusCode.CreateResponse(result);

        // Assert
        Assert.IsType<ApiResponseDto<string>>(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(result, response.Result);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void CreateResponse_Generic_WithNullResult_ReturnsApiResponseDtoWithNullResult()
    {
        // Act
        var response = HttpStatusCode.OK.CreateResponse<string>(null);

        // Assert
        Assert.IsType<ApiResponseDto<string>>(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Result);
    }
}
