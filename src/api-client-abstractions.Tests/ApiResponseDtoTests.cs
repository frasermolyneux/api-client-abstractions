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

    [Fact]
    public void StatusCodeAndMultipleErrorsConstructor_ShouldSetStatusCodeAndAllErrors()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.BadRequest;
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var apiResponse = new ApiResponseDto(statusCode, errors);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.Equal(errors, apiResponse.Errors);
        Assert.Equal(3, apiResponse.Errors.Count);
    }

    [Fact]
    public void Constructor_WithNullErrors_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiResponseDto(HttpStatusCode.OK, null!));
    }

    [Fact]
    public void IsSuccess_WithRedirectionStatusAndNoErrors_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.Redirect);

        // Act & Assert
        Assert.False(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithCreatedStatusAndNoErrors_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.Created);

        // Act & Assert
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsNotFound_WithGoneStatus_ShouldReturnFalse()
    {
        // Arrange - Gone (410) is similar to but not the same as NotFound (404)
        var apiResponse = new ApiResponseDto(HttpStatusCode.Gone);

        // Act & Assert
        Assert.False(apiResponse.IsNotFound);
    }

    [Fact]
    public void IsSuccess_WithNullErrorsReference_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto(HttpStatusCode.OK);
        apiResponse.Errors = null!; // Force null for testing

        // Act & Assert - should not throw exception
        Assert.True(apiResponse.IsSuccess);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.NoContent, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsSuccess_WithVariousStatusCodes_ShouldReturnExpectedResult(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        var apiResponse = new ApiResponseDto(statusCode);

        // Act & Assert
        Assert.Equal(expectedResult, apiResponse.IsSuccess);
    }

    [Fact]
    public void StatusCodeAndNullErrorConstructor_ShouldNotAddError()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.BadRequest;
        var apiResponse = new ApiResponseDto(statusCode, null!);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
    }

    [Fact]
    public void JsonSerialization_ShouldPreserveProperties()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        var errorMessage = "Test error message";
        var apiResponse = new ApiResponseDto(statusCode, errorMessage);

        // Act
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(apiResponse);
        var deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseDto>(json);

        // Assert
        Assert.NotNull(deserializedResponse);
        Assert.Equal(statusCode, deserializedResponse.StatusCode);
        Assert.NotNull(deserializedResponse.Errors);
        Assert.Single(deserializedResponse.Errors);
        Assert.Equal(errorMessage, deserializedResponse.Errors[0]);
    }
}
