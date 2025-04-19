using System.Net;
using MxIO.ApiClient.Abstractions;
using Xunit;

namespace MxIO.ApiClient.Abstractions.Tests;

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
        Assert.Throws<ArgumentNullException>(() => new ApiResponseDto<string>(HttpStatusCode.OK, "Result", errors: null!));
    }

    [Fact]
    public void IsSuccess_WithCreatedStatusNoErrorsAndNonNullResult_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.Created, "Result");

        // Act & Assert
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithAcceptedStatusNoErrorsAndNonNullResult_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.Accepted, "Result");

        // Act & Assert
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithRedirectionStatusNoErrorsAndNonNullResult_ShouldReturnFalse()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.Redirect, "Result");

        // Act & Assert
        Assert.False(apiResponse.IsSuccess);
    }

    [Fact]
    public void Constructor_WithCustomStatusCodeErrorsAndResult_SetsPropertiesCorrectly()
    {
        // Arrange
        var statusCode = HttpStatusCode.Conflict;
        var errors = new List<string> { "Error 1", "Error 2" };
        var result = new TestObject { Id = 42, Name = "Test" };

        // Act
        var apiResponse = new ApiResponseDto<TestObject>(statusCode, result, errors);

        // Assert
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.Same(result, apiResponse.Result);
        Assert.Equal(errors, apiResponse.Errors);
        Assert.Equal(2, apiResponse.Errors.Count);
    }

    [Fact]
    public void IsNotFound_WithNotFoundStatusAndResult_ShouldReturnTrue()
    {
        // Arrange - Even with a result, NotFound status should make IsNotFound true
        var apiResponse = new ApiResponseDto<string>(HttpStatusCode.NotFound, "Some data");

        // Act & Assert
        Assert.True(apiResponse.IsNotFound);
    }

    [Fact]
    public void ApiResponseWithNonStringType_ConstructsAndSerializesCorrectly()
    {
        // Arrange
        var complexResult = new TestObject { Id = 123, Name = "Test Complex Object" };

        // Act
        var apiResponse = new ApiResponseDto<TestObject>(HttpStatusCode.OK, complexResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(123, apiResponse.Result.Id);
        Assert.Equal("Test Complex Object", apiResponse.Result.Name);
        Assert.Empty(apiResponse.Errors);
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void StatusCodeAndNullErrorConstructor_ShouldNotAddError()
    {
        // Arrange & Act
        var statusCode = HttpStatusCode.BadRequest;
        var apiResponse = new ApiResponseDto<string>(statusCode, null!);

        // Assert - String.Empty is different from null
        Assert.Equal(statusCode, apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Errors);
        Assert.Empty(apiResponse.Errors);
    }

    [Fact]
    public void IsSuccess_WithOkStatusNoErrorsAndValueTypeResult_ShouldReturnTrue()
    {
        // Arrange
        var apiResponse = new ApiResponseDto<int>(HttpStatusCode.OK, 42);

        // Act & Assert
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WithOkStatusNoErrorsAndDefaultValueTypeResult_ShouldReturnTrue()
    {
        // Arrange - Default value (0) for int should still count as a valid result
        var apiResponse = new ApiResponseDto<int>(HttpStatusCode.OK, 0);

        // Act & Assert
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public void JsonSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var result = "Test result";
        var errors = new List<string> { "Warning 1" };
        var apiResponse = new ApiResponseDto<string>(statusCode, result, errors);

        // Act
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(apiResponse);
        var deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseDto<string>>(json);

        // Assert
        Assert.NotNull(deserializedResponse);
        Assert.Equal(statusCode, deserializedResponse.StatusCode);
        Assert.Equal(result, deserializedResponse.Result);
        Assert.NotNull(deserializedResponse.Errors);
        Assert.Single(deserializedResponse.Errors);
        Assert.Equal("Warning 1", deserializedResponse.Errors[0]);
    }

    [Fact]
    public void JsonSerialization_WithComplexType_ShouldPreserveAllProperties()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var result = new TestObject { Id = 42, Name = "Test Object" };
        var apiResponse = new ApiResponseDto<TestObject>(statusCode, result);

        // Act
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(apiResponse);
        var deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseDto<TestObject>>(json);

        // Assert
        Assert.NotNull(deserializedResponse);
        Assert.Equal(statusCode, deserializedResponse.StatusCode);
        Assert.NotNull(deserializedResponse.Result);
        Assert.Equal(42, deserializedResponse.Result!.Id);
        Assert.Equal("Test Object", deserializedResponse.Result.Name);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.NoContent, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsSuccess_WithVariousStatusCodes_AndNonNullResult_ReturnsExpected(HttpStatusCode statusCode, bool expectedResult)
    {
        // Arrange
        var apiResponse = new ApiResponseDto<string>(statusCode, "Test");

        // Act & Assert
        Assert.Equal(expectedResult, apiResponse.IsSuccess);
    }

    // Test class for complex object testing
    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}