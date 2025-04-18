using System.Net;
using MxIO.ApiClient.Abstractions;
using Xunit;

namespace MxIO.ApiClient.Abstractions.Tests;

public class ApiResponseDtoTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyErrors()
    {
        // Arrange & Act
        var apiResponse = new ApiResponseDto();

        // Assert
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
    }

    [Fact]
    public void StatusCodeConstructor_ShouldSetStatusCode()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.OK;
        var apiResponse = new ApiResponseDto(statusCode);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
    }

    [Fact]
    public void StatusCodeAndErrorConstructor_ShouldSetStatusCodeAndError()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.BadRequest;
        var errorMessage = "Test error message";
        var apiResponse = new ApiResponseDto(statusCode, errorMessage);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Single(apiResponse.Errors);
        Assert.Equal(errorMessage, apiResponse.Errors[0]);
    }

    [Fact]
    public void StatusCodeAndEmptyErrorConstructor_ShouldSetStatusCodeOnly()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.BadRequest;
        var apiResponse = new ApiResponseDto(statusCode, string.Empty);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
    }

    [Fact]
    public void IsSuccess_WithOkStatusAndNoErrors_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.OK);

        // Act & Assert
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithOkStatusAndErrors_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.OK, "Error message");

        // Act & Assert
        Assert.False(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithNonOkStatusAndNoErrors_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.BadRequest);

        // Act & Assert
        Assert.False(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsNotFound_WithNotFoundStatus_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.NotFound);

        // Act & Assert
        Assert.True(apiResponse.IsNotFound);
    }

    [Fact]
    public void IsNotFound_WithNonNotFoundStatus_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.OK);

        // Act & Assert
        Assert.False(apiResponse.IsNotFound);
    }
}
