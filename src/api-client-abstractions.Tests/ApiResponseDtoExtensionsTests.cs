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

    [Fact]
    public void ToHttpResult_Generic_WithErrorStatusCode_ReturnsObjectResultWithCorrectStatusCode()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.BadRequest, "Error result", new List<string> { "Validation error" });

        // Act
        var result = apiResponse.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        Assert.Same(apiResponse, objectResult.Value);
    }

    [Fact]
    public void ToHttpResult_NonGeneric_WithServerErrorStatusCode_ReturnsObjectResultWithCorrectStatusCode()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = apiResponse.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        Assert.Same(apiResponse, objectResult.Value);
    }

    [Fact]
    public void CreateResponse_FromStatusCodeWithErrorMessage_ShouldCreateApiResponseWithErrorMessage()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        var errorMessage = "Validation error";

        // Act
        var response = new ApiResponseDto(statusCode, errorMessage);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Contains(errorMessage, response.Errors);
        Assert.Single(response.Errors);
    }

    [Fact]
    public void CreateResponse_FromStatusCodeWithResultAndErrorMessage_ShouldCreateApiResponseWithResultAndError()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var result = "Test Result";
        var errors = new List<string> { "Warning message" };

        // Act
        var response = new ApiResponseDto<string>(statusCode, result, errors);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(result, response.Result);
        Assert.Equal(errors, response.Errors);
        Assert.Single(response.Errors);
    }

    [Fact]
    public void ToHttpResult_Generic_WithComplexType_ShouldReturnObjectResultWithSameValue()
    {
        // Arrange
        var complexObject = new { Id = 1, Name = "Test" };
        var apiResponse = new ApiResponseDto<object>(HttpStatusCode.OK, complexObject);

        // Act
        var result = apiResponse.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
        Assert.Same(apiResponse, objectResult.Value);
        Assert.Same(complexObject, ((ApiResponseDto<object>)objectResult.Value).Result);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.Unauthorized, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void CreateResponse_WithDifferentStatusCodes_SetsIsSuccessCorrectly(HttpStatusCode statusCode, bool expectedIsSuccess)
    {
        // Arrange & Act
        var response = statusCode.CreateResponse();

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(expectedIsSuccess, response.IsSuccess);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.NotFound, true)]
    public void CreateResponse_WithDifferentStatusCodes_SetsIsNotFoundCorrectly(HttpStatusCode statusCode, bool expectedIsNotFound)
    {
        // Arrange & Act
        var response = statusCode.CreateResponse();

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(expectedIsNotFound, response.IsNotFound);
    }

    [Fact]
    public void CreateResponse_WithGenericType_WithValueType_ShouldCreateCorrectly()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var result = 42;

        // Act
        var response = statusCode.CreateResponse(result);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(result, response.Result);
    }
}