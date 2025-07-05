using System.Net;

namespace MX.Api.Abstractions.Tests;

public class HttpResponseWrapperTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var wrapper = new HttpResponseWrapper();

        // Assert
        Assert.Equal(default(HttpStatusCode), wrapper.StatusCode);
        Assert.False(wrapper.IsSuccess);
        Assert.False(wrapper.IsNotFound);
        Assert.False(wrapper.IsConflict);
    }

    [Fact]
    public void Constructor_WithStatusCode_SetsStatusCode()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act
        var wrapper = new HttpResponseWrapper(statusCode);

        // Assert
        Assert.Equal(statusCode, wrapper.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsSuccess_ReturnsCorrectValue_ForDifferentStatusCodes(HttpStatusCode statusCode, bool expectedIsSuccess)
    {
        // Act
        var wrapper = new HttpResponseWrapper(statusCode);

        // Assert
        Assert.Equal(expectedIsSuccess, wrapper.IsSuccess);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsNotFound_ReturnsCorrectValue_ForDifferentStatusCodes(HttpStatusCode statusCode, bool expectedIsNotFound)
    {
        // Act
        var wrapper = new HttpResponseWrapper(statusCode);

        // Assert
        Assert.Equal(expectedIsNotFound, wrapper.IsNotFound);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsConflict_ReturnsCorrectValue_ForDifferentStatusCodes(HttpStatusCode statusCode, bool expectedIsConflict)
    {
        // Act
        var wrapper = new HttpResponseWrapper(statusCode);

        // Assert
        Assert.Equal(expectedIsConflict, wrapper.IsConflict);
    }

    [Fact]
    public void GenericHttpResponseWrapper_DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var wrapper = new HttpResponseWrapper<string>();

        // Assert
        Assert.Equal(default(HttpStatusCode), wrapper.StatusCode);
        Assert.False(wrapper.IsSuccess);
        Assert.Null(wrapper.Result);
    }

    [Fact]
    public void GenericHttpResponseWrapper_ConstructorWithStatusCode_SetsStatusCode()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act
        var wrapper = new HttpResponseWrapper<string>(statusCode);

        // Assert
        Assert.Equal(statusCode, wrapper.StatusCode);
        Assert.False(wrapper.IsSuccess); // IsSuccess is overridden in generic class to require a non-null Result
        Assert.Null(wrapper.Result);
    }

    [Fact]
    public void GenericHttpResponseWrapper_ConstructorWithStatusCodeAndResult_SetsStatusCodeAndResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var apiResponse = new ApiResponse<string>("Test data");

        // Act
        var wrapper = new HttpResponseWrapper<string>(statusCode, apiResponse);

        // Assert
        Assert.Equal(statusCode, wrapper.StatusCode);
        Assert.True(wrapper.IsSuccess);
        Assert.Equal(apiResponse, wrapper.Result);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void GenericHttpResponseWrapper_IsSuccess_RequiresBothSuccessStatusCodeAndNonNullResult(HttpStatusCode statusCode, bool baseIsSuccess)
    {
        // Arrange
        var wrapperWithNullResult = new HttpResponseWrapper<string>(statusCode);
        var wrapperWithResult = new HttpResponseWrapper<string>(statusCode, new ApiResponse<string>());

        // Assert
        Assert.False(wrapperWithNullResult.IsSuccess);
        Assert.Equal(baseIsSuccess, wrapperWithResult.IsSuccess);
    }

    [Fact]
    public void GenericHttpResponseWrapper_Implementation_ImplementsInterface()
    {
        // Act
        var wrapper = new HttpResponseWrapper<string>();

        // Assert
        Assert.IsAssignableFrom<IHttpResponseWrapper>(wrapper);
        Assert.IsAssignableFrom<IHttpResponseWrapper<string>>(wrapper);
    }
}
