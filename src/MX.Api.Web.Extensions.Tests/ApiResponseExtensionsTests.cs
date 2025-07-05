using System.Net;
using MX.Api.Abstractions;

namespace MX.Api.Web.Extensions.Tests;

public class ApiResponseExtensionsTests
{
    [Fact]
    public void CreateApiResponse_Generic_ReturnsApiResponse()
    {
        // Act
        var result = ApiResponseExtensions.CreateApiResponse<string>();

        // Assert
        Assert.IsType<ApiResponse<string>>(result);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
        Assert.Null(result.Pagination);
    }

    [Fact]
    public void CreateApiResponse_WithData_ReturnsApiResponse()
    {
        // Arrange
        var data = "Test Data";

        // Act
        var result = ApiResponseExtensions.CreateApiResponse(data);

        // Assert
        Assert.IsType<ApiResponse<string>>(result);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
        Assert.Null(result.Pagination);
    }

    [Fact]
    public void CreateApiResponse_WithDataAndPagination_ReturnsApiResponse()
    {
        // Arrange
        var data = "Test Data";
        var pagination = new ApiPagination(100, 50, 0, 10);

        // Act
        var result = ApiResponseExtensions.CreateApiResponse(data, pagination);

        // Assert
        Assert.IsType<ApiResponse<string>>(result);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
        Assert.NotNull(result.Pagination);
        Assert.Equal(pagination, result.Pagination);
    }

    [Fact]
    public void CreateApiErrorResponse_WithError_ReturnsApiResponse()
    {
        // Arrange
        var error = new ApiError("ValidationFailed", "Validation failed");

        // Act
        var result = ApiResponseExtensions.CreateApiErrorResponse<string>(error);

        // Assert
        Assert.IsType<ApiResponse<string>>(result);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Equal(error.Code, result.Errors[0].Code);
        Assert.Equal(error.Message, result.Errors[0].Message);
        Assert.Null(result.Pagination);
    }

    [Fact]
    public void CreateApiErrorResponse_WithNullError_ThrowsArgumentNullException()
    {
        // Arrange
        ApiError? error = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ApiResponseExtensions.CreateApiErrorResponse<string>(error!));
    }

    [Fact]
    public void CreateApiErrorResponse_WithMessage_ReturnsApiResponse()
    {
        // Arrange
        var message = "Validation failed";

        // Act
        var result = ApiResponseExtensions.CreateApiErrorResponse<string>(message);

        // Assert
        Assert.IsType<ApiResponse<string>>(result);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Equal("Error", result.Errors[0].Code);
        Assert.Equal(message, result.Errors[0].Message);
        Assert.Null(result.Pagination);
    }

    [Fact]
    public void CreateApiCollectionResponse_ReturnsApiResponse()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };
        var totalCount = 100;
        var filteredCount = 50;
        var skip = 0;
        var top = 10;

        // Act
        var result = ApiResponseExtensions.CreateApiCollectionResponse(items, totalCount, filteredCount, skip, top);

        // Assert
        Assert.IsType<ApiResponse<CollectionModel<string>>>(result);
        Assert.NotNull(result.Data);
        Assert.Equal(items, result.Data.Items);
        Assert.Equal(totalCount, result.Data.TotalCount);
        Assert.Equal(filteredCount, result.Data.FilteredCount);
        Assert.NotNull(result.Pagination);
        Assert.Equal(totalCount, result.Pagination.TotalCount);
        Assert.Equal(filteredCount, result.Pagination.FilteredCount);
        Assert.Equal(skip, result.Pagination.Skip);
        Assert.Equal(top, result.Pagination.Top);
        Assert.True(result.Pagination.HasMore); // 50 > 0 + 10
    }

    [Fact]
    public void CreateApiCountResponse_ReturnsApiResponse()
    {
        // Arrange
        var count = 42;

        // Act
        var result = ApiResponseExtensions.CreateApiCountResponse(count);

        // Assert
        Assert.IsType<ApiResponse<int>>(result);
        Assert.Equal(count, result.Data);
        Assert.Null(result.Errors);
        Assert.Null(result.Pagination);
    }
}
