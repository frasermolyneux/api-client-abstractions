using System.Net;

using MX.Api.Abstractions;
using MX.Api.Web.Extensions;

namespace MX.Api.Web.Extensions.Tests;

/// <summary>
/// Unit tests for ApiResponseExtensions methods.
/// </summary>
public class ApiResponseExtensionsTests
{
    #region ToApiResult Tests

    [Fact]
    public void ToApiResult_WithValidApiResponse_ReturnsCorrectApiResult()
    {
        // Arrange
        var apiResponse = new ApiResponse();
        var expectedStatusCode = HttpStatusCode.OK;

        // Act
        var result = apiResponse.ToApiResult(expectedStatusCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStatusCode, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToApiResult_WithDefaultStatusCode_ReturnsOkStatus()
    {
        // Arrange
        var apiResponse = new ApiResponse();

        // Act
        var result = apiResponse.ToApiResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToApiResult_WithNullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToApiResult());
    }

    [Fact]
    public void ToApiResult_Generic_WithValidApiResponse_ReturnsCorrectApiResult()
    {
        // Arrange
        var testData = "test data";
        var apiResponse = new ApiResponse<string>(testData);
        var expectedStatusCode = HttpStatusCode.Created;

        // Act
        var result = apiResponse.ToApiResult(expectedStatusCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStatusCode, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
        Assert.Equal(testData, result.Result?.Data);
    }

    [Fact]
    public void ToApiResult_Generic_WithNullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse<string>? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToApiResult());
    }

    #endregion

    #region ToApiResultWithErrorHandling Tests

    [Fact]
    public void ToApiResultWithErrorHandling_WithNoErrors_ReturnsOkStatus()
    {
        // Arrange
        var apiResponse = new ApiResponse();

        // Act
        var result = apiResponse.ToApiResultWithErrorHandling();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToApiResultWithErrorHandling_WithErrors_ReturnsBadRequestStatus()
    {
        // Arrange
        var error = new ApiError { Code = "TEST_ERROR", Message = "Test error message" };
        var apiResponse = new ApiResponse(error);

        // Act
        var result = apiResponse.ToApiResultWithErrorHandling();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToApiResultWithErrorHandling_Generic_WithDataAndNoErrors_ReturnsOkStatus()
    {
        // Arrange
        var testData = "test data";
        var apiResponse = new ApiResponse<string>(testData);

        // Act
        var result = apiResponse.ToApiResultWithErrorHandling();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToApiResultWithErrorHandling_Generic_WithNullDataAndNoErrors_ReturnsNotFoundStatus()
    {
        // Arrange
        var apiResponse = new ApiResponse<string>((string?)null);

        // Act
        var result = apiResponse.ToApiResultWithErrorHandling();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToApiResultWithErrorHandling_Generic_WithErrors_ReturnsBadRequestStatus()
    {
        // Arrange
        var error = new ApiError { Code = "TEST_ERROR", Message = "Test error message" };
        var apiResponse = new ApiResponse<string>("test data", error);

        // Act
        var result = apiResponse.ToApiResultWithErrorHandling();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    #endregion

    #region ToCreatedResult Tests

    [Fact]
    public void ToCreatedResult_WithValidApiResponse_ReturnsCreatedStatus()
    {
        // Arrange
        var apiResponse = new ApiResponse();

        // Act
        var result = apiResponse.ToCreatedResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToCreatedResult_Generic_WithValidApiResponse_ReturnsCreatedStatus()
    {
        // Arrange
        var testData = "test data";
        var apiResponse = new ApiResponse<string>(testData);

        // Act
        var result = apiResponse.ToCreatedResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
        Assert.Equal(testData, result.Result?.Data);
    }

    [Fact]
    public void ToCreatedResult_WithNullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToCreatedResult());
    }

    #endregion

    #region ToAcceptedResult Tests

    [Fact]
    public void ToAcceptedResult_WithValidApiResponse_ReturnsAcceptedStatus()
    {
        // Arrange
        var apiResponse = new ApiResponse();

        // Act
        var result = apiResponse.ToAcceptedResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToAcceptedResult_Generic_WithValidApiResponse_ReturnsAcceptedStatus()
    {
        // Arrange
        var testData = "test data";
        var apiResponse = new ApiResponse<string>(testData);

        // Act
        var result = apiResponse.ToAcceptedResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
        Assert.Equal(testData, result.Result?.Data);
    }

    [Fact]
    public void ToAcceptedResult_WithNullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToAcceptedResult());
    }

    #endregion

    #region ToNotFoundResult Tests

    [Fact]
    public void ToNotFoundResult_WithValidApiResponse_ReturnsNotFoundStatus()
    {
        // Arrange
        var apiResponse = new ApiResponse();

        // Act
        var result = apiResponse.ToNotFoundResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToNotFoundResult_Generic_WithValidApiResponse_ReturnsNotFoundStatus()
    {
        // Arrange
        var apiResponse = new ApiResponse<string>((string?)null);

        // Act
        var result = apiResponse.ToNotFoundResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToNotFoundResult_WithNullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToNotFoundResult());
    }

    #endregion

    #region ToBadRequestResult Tests

    [Fact]
    public void ToBadRequestResult_WithValidApiResponse_ReturnsBadRequestStatus()
    {
        // Arrange
        var error = new ApiError { Code = "VALIDATION_ERROR", Message = "Validation failed" };
        var apiResponse = new ApiResponse(error);

        // Act
        var result = apiResponse.ToBadRequestResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToBadRequestResult_Generic_WithValidApiResponse_ReturnsBadRequestStatus()
    {
        // Arrange
        var error = new ApiError { Code = "VALIDATION_ERROR", Message = "Validation failed" };
        var apiResponse = new ApiResponse<string>("test data", error);

        // Act
        var result = apiResponse.ToBadRequestResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
        Assert.Equal("test data", result.Result?.Data);
    }

    [Fact]
    public void ToBadRequestResult_WithNullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToBadRequestResult());
    }

    #endregion

    #region ToConflictResult Tests

    [Fact]
    public void ToConflictResult_WithValidApiResponse_ReturnsConflictStatus()
    {
        // Arrange
        var error = new ApiError { Code = "CONFLICT_ERROR", Message = "Resource conflict" };
        var apiResponse = new ApiResponse(error);

        // Act
        var result = apiResponse.ToConflictResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
    }

    [Fact]
    public void ToConflictResult_Generic_WithValidApiResponse_ReturnsConflictStatus()
    {
        // Arrange
        var error = new ApiError { Code = "CONFLICT_ERROR", Message = "Resource conflict" };
        var apiResponse = new ApiResponse<string>("test data", error);

        // Act
        var result = apiResponse.ToConflictResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(apiResponse, result.Result);
        Assert.Equal("test data", result.Result?.Data);
    }

    [Fact]
    public void ToConflictResult_WithNullApiResponse_ThrowsArgumentNullException()
    {
        // Arrange
        ApiResponse? apiResponse = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => apiResponse!.ToConflictResult());
    }

    #endregion
}
