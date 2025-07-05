using System.Net;
using Microsoft.AspNetCore.Mvc;
using MX.Api.Abstractions;

namespace MX.Api.Web.Extensions.Tests;

public class HttpResponseExtensionsTests
{
    [Fact]
    public void ToHttpResult_Generic_WithApiResponse_ReturnsObjectResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var data = "Test Data";
        var apiResponse = new ApiResponse<string>(data);
        var responseWrapper = new ApiResult<string>(statusCode, apiResponse);

        // Act
        var result = responseWrapper.ToHttpResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)statusCode, objectResult.StatusCode);
        Assert.Equal(apiResponse, objectResult.Value);
    }

    [Fact]
    public void ToHttpResult_Generic_WithNullApiResponse_ReturnsStatusCodeResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.NoContent;
        var responseWrapper = new ApiResult<string>(statusCode);

        // Act
        var result = responseWrapper.ToHttpResult();

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)statusCode, statusCodeResult.StatusCode);
    }

    [Fact]
    public void ToHttpResult_Generic_WithNullWrapper_ThrowsArgumentNullException()
    {
        // Arrange
        IApiResult<string>? apiResult = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => apiResult!.ToHttpResult());
        Assert.Equal("apiResult", exception.ParamName);
    }

    [Fact]
    public void ToHttpResult_NonGeneric_ReturnsStatusCodeResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var responseWrapper = new ApiResult(statusCode);

        // Act
        var result = responseWrapper.ToHttpResult();

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)statusCode, statusCodeResult.StatusCode);
    }

    [Fact]
    public void ToHttpResult_NonGeneric_WithNullWrapper_ThrowsArgumentNullException()
    {
        // Arrange
        IApiResult? apiResult = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => apiResult!.ToHttpResult());
        Assert.Equal("apiResult", exception.ParamName);
    }
}
