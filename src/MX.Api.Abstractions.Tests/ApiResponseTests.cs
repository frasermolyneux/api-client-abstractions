using System.Net;
using System.Collections.Generic;

namespace MX.Api.Abstractions.Tests;

public class ApiResponseTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var response = new ApiResponse<string>();

        // Assert
        Assert.Null(response.Data);
        Assert.Null(response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithData_SetsData()
    {
        // Arrange
        var data = "Test Data";

        // Act
        var response = new ApiResponse<string>(data);

        // Assert
        Assert.Equal(data, response.Data);
        Assert.Null(response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithError_SetsErrors()
    {
        // Arrange
        var error = new ApiError("TestCode", "Test error message");

        // Act
        var response = new ApiResponse<string>(error);

        // Assert
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
        ApiError? error = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ApiResponse<string>(error!));
        Assert.Equal("error", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithErrors_SetsErrorsArray()
    {
        // Arrange
        var errors = new[]
        {
            new ApiError("Code1", "Message1"),
            new ApiError("Code2", "Message2")
        };

        // Act
        var response = new ApiResponse<string>(errors);

        // Assert
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
        ApiError[]? errors = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ApiResponse<string>(errors!));
        Assert.Equal("errors", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDataAndError_SetsAllValues()
    {
        // Arrange
        var data = "Test Data";
        var error = new ApiError("TestCode", "Test error message");

        // Act
        var response = new ApiResponse<string>(data, error);

        // Assert
        Assert.Equal(data, response.Data);
        Assert.NotNull(response.Errors);
        Assert.Single(response.Errors);
        Assert.Equal(error, response.Errors[0]);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithDataAndErrors_SetsAllValues()
    {
        // Arrange
        var data = "Test Data";
        var errors = new[]
        {
            new ApiError("Code1", "Message1"),
            new ApiError("Code2", "Message2")
        };

        // Act
        var response = new ApiResponse<string>(data, errors);

        // Assert
        Assert.Equal(data, response.Data);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Length);
        Assert.Equal(errors, response.Errors);
        Assert.Null(response.Pagination);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var response = new ApiResponse<string>();
        var data = "Test Data";
        var errors = new[] { new ApiError("ERROR", "Error message") };
        var pagination = new ApiPagination(100, 50, 0, 10);
        var metadata = new Dictionary<string, string> { { "key", "value" } };

        // Act
        response.Data = data;
        response.Errors = errors;
        response.Pagination = pagination;
        response.Metadata = metadata;

        // Assert
        Assert.Equal(data, response.Data);
        Assert.Equal(errors, response.Errors);
        Assert.Equal(pagination, response.Pagination);
        Assert.Equal(metadata, response.Metadata);
    }
}

public class ApiResponseNonGenericTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var response = new ApiResponse();

        // Assert
        Assert.Null(response.Errors);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithError_SetsError()
    {
        // Arrange
        var error = new ApiError("TEST_ERROR", "Test error");

        // Act
        var response = new ApiResponse(error);

        // Assert
        Assert.NotNull(response.Errors);
        Assert.Single(response.Errors);
        Assert.Equal(error, response.Errors[0]);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithErrors_SetsErrors()
    {
        // Arrange
        var errors = new[]
        {
            new ApiError("ERROR_1", "Error 1"),
            new ApiError("ERROR_2", "Error 2")
        };

        // Act
        var response = new ApiResponse(errors);

        // Assert
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Length);
        Assert.Equal(errors, response.Errors);
        Assert.Null(response.Metadata);
    }

    [Fact]
    public void Constructor_WithNullError_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiResponse((ApiError)null!));
    }

    [Fact]
    public void Constructor_WithNullErrors_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiResponse((ApiError[])null!));
    }

    [Fact]
    public void Errors_CanBeSetAndRetrieved()
    {
        // Arrange
        var response = new ApiResponse();
        var errors = new[]
        {
            new ApiError("TEST_ERROR", "Test error")
        };

        // Act
        response.Errors = errors;

        // Assert
        Assert.Equal(errors, response.Errors);
    }

    [Fact]
    public void Metadata_CanBeSetAndRetrieved()
    {
        // Arrange
        var response = new ApiResponse();
        var metadata = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        response.Metadata = metadata;

        // Assert
        Assert.Equal(metadata, response.Metadata);
    }
}
