using System.Net;

namespace MX.Api.Abstractions.Tests;

public class ApiResponseTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var response = new ApiResponse<string>();

        // Assert
        Assert.Equal(default(HttpStatusCode), response.StatusCode);
        Assert.Null(response.Data);
        Assert.Null(response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithStatusCode_SetsStatusCode()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act
        var response = new ApiResponse<string>(statusCode);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Null(response.Data);
        Assert.Null(response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithStatusCodeAndData_SetsStatusCodeAndData()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var data = "Test Data";

        // Act
        var response = new ApiResponse<string>(statusCode, data);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(data, response.Data);
        Assert.Null(response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithStatusCodeAndError_SetsStatusCodeAndErrors()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        var error = new ApiError("TestCode", "Test error message");

        // Act
        var response = new ApiResponse<string>(statusCode, error);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Null(response.Data);
        Assert.NotNull(response.Errors);
        Assert.Single(response.Errors);
        Assert.Equal(error, response.Errors[0]);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithNullError_ThrowsArgumentNullException()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        ApiError? error = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ApiResponse<string>(statusCode, error!));
        Assert.Equal("error", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithStatusCodeAndErrors_SetsStatusCodeAndErrorsArray()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        var errors = new[]
        {
            new ApiError("Code1", "Message1"),
            new ApiError("Code2", "Message2")
        };

        // Act
        var response = new ApiResponse<string>(statusCode, errors);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Null(response.Data);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Length);
        Assert.Equal(errors, response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithNullErrorsArray_ThrowsArgumentNullException()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        ApiError[]? errors = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ApiResponse<string>(statusCode, errors!));
        Assert.Equal("errors", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithStatusCodeDataAndError_SetsAllValues()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        var data = "Test Data";
        var error = new ApiError("TestCode", "Test error message");

        // Act
        var response = new ApiResponse<string>(statusCode, data, error);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(data, response.Data);
        Assert.NotNull(response.Errors);
        Assert.Single(response.Errors);
        Assert.Equal(error, response.Errors[0]);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithStatusCodeDataAndErrors_SetsAllValues()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        var data = "Test Data";
        var errors = new[]
        {
            new ApiError("Code1", "Message1"),
            new ApiError("Code2", "Message2")
        };

        // Act
        var response = new ApiResponse<string>(statusCode, data, errors);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(data, response.Data);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Length);
        Assert.Equal(errors, response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }
}
