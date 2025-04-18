using System.Net;
using MxIO.ApiClient.Abstractions;
using Xunit;

namespace api_client_abstractions.Tests;

public class ApiResponseDtoGenericTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyErrors()
    {
        // Arrange & Act
        var apiResponse = new ApiResponseDto<string>();

        // Assert
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
        Assert.Null(apiResponse.Result);
    }

    [Fact]
    public void StatusCodeConstructor_ShouldSetStatusCode()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.OK;
        var apiResponse = new ApiResponseDto<int>(statusCode);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
        Assert.Equal(default, apiResponse.Result);
    }

    [Fact]
    public void StatusCodeAndErrorConstructor_ShouldSetStatusCodeAndError()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.BadRequest;
        var errorMessage = "Test error message";
        var apiResponse = new ApiResponseDto<string>(statusCode, errorMessage);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Single(apiResponse.Errors);
        Assert.Equal(errorMessage, apiResponse.Errors[0]);
        Assert.Null(apiResponse.Result);
    }

    [Fact]
    public void StatusCodeAndResultConstructor_ShouldSetStatusCodeAndResult()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.OK;
        var result = "Test result";
        var apiResponse = new ApiResponseDto<string>(statusCode, result);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
        Assert.Equal(result, apiResponse.Result);
    }

    [Fact]
    public void StatusCodeResultAndErrorsConstructor_ShouldSetStatusCodeResultAndErrors()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.OK;
        var result = "Test result";
        var errors = new List<string> { "Error 1", "Error 2" };
        var apiResponse = new ApiResponseDto<string>(statusCode, result, errors);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.Equal(errors, apiResponse.Errors);
        Assert.Equal(result, apiResponse.Result);
        Assert.Equal(2, apiResponse.Errors.Count);
    }

    [Fact]
    public void IsSuccess_WithOkStatusNoErrorsAndNonNullResult_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.OK, "Result");

        // Act & Assert
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithOkStatusNoErrorsButNullResult_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.OK, null);

        // Act & Assert
        Assert.False(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithOkStatusWithErrorsAndNonNullResult_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.OK, "Result", new List<string> { "Error" });

        // Act & Assert
        Assert.False(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithNonOkStatusWithNonNullResult_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.BadRequest, "Result");

        // Act & Assert
        Assert.False(apiResponse.IsSuccess);
    }

    [Fact]
    public void Constructor_WithNullErrors_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiResponseDto<string>(HttpStatusCode.OK, "Result", null!));
    }
}