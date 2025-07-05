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
        Assert.Null(wrapper.Result);
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
        Assert.Null(wrapper.Result);
    }

    [Fact]
    public void Constructor_WithStatusCodeAndApiResponse_SetsStatusCodeAndResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var apiResponse = new ApiResponse();

        // Act
        var wrapper = new HttpResponseWrapper(statusCode, apiResponse);

        // Assert
        Assert.Equal(statusCode, wrapper.StatusCode);
        Assert.Equal(apiResponse, wrapper.Result);
        Assert.True(wrapper.IsSuccess);
    }

    [Fact]
    public void Constructor_WithStatusCodeAndNullApiResponse_SetsStatusCodeAndNullResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        ApiResponse? apiResponse = null;

        // Act
        var wrapper = new HttpResponseWrapper(statusCode, apiResponse);

        // Assert
        Assert.Equal(statusCode, wrapper.StatusCode);
        Assert.Null(wrapper.Result);
        Assert.True(wrapper.IsSuccess); // IsSuccess in base class only depends on status code
    }

    [Fact]
    public void HttpResponseWrapper_Implementation_ImplementsInterface()
    {
        // Act
        var wrapper = new HttpResponseWrapper();

        // Assert
        Assert.IsAssignableFrom<IHttpResponseWrapper>(wrapper);
    }

    [Fact]
    public void HttpResponseWrapper_ResultProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var wrapper = new HttpResponseWrapper();
        var apiResponse = new ApiResponse();

        // Act
        wrapper.Result = apiResponse;

        // Assert
        Assert.Equal(apiResponse, wrapper.Result);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void HttpResponseWrapper_IsSuccess_DependsOnlyOnStatusCode(HttpStatusCode statusCode, bool expectedIsSuccess)
    {
        // Arrange
        var wrapperWithNullResult = new HttpResponseWrapper(statusCode);
        var wrapperWithResult = new HttpResponseWrapper(statusCode, new ApiResponse());

        // Assert - Both should have the same IsSuccess value regardless of Result
        Assert.Equal(expectedIsSuccess, wrapperWithNullResult.IsSuccess);
        Assert.Equal(expectedIsSuccess, wrapperWithResult.IsSuccess);
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

    [Fact]
    public void GenericHttpResponseWrapper_InheritsFromHttpResponseWrapper()
    {
        // Act
        var wrapper = new HttpResponseWrapper<string>();

        // Assert
        Assert.IsAssignableFrom<HttpResponseWrapper>(wrapper);
        Assert.IsAssignableFrom<IHttpResponseWrapper>(wrapper);
        Assert.IsAssignableFrom<IHttpResponseWrapper<string>>(wrapper);
    }

    [Fact]
    public void HttpResponseWrapper_And_GenericHttpResponseWrapper_CanBeUsedPolymorphically()
    {
        // Arrange
        IHttpResponseWrapper baseWrapper = new HttpResponseWrapper(HttpStatusCode.OK, new ApiResponse());
        IHttpResponseWrapper<string> genericWrapper = new HttpResponseWrapper<string>(HttpStatusCode.OK, new ApiResponse<string>("test"));

        // Assert
        Assert.True(baseWrapper.IsSuccess);
        Assert.True(genericWrapper.IsSuccess);
        Assert.NotNull(baseWrapper.Result);
        Assert.NotNull(genericWrapper.Result);

        // Verify types
        Assert.IsType<ApiResponse>(baseWrapper.Result);
        Assert.IsType<ApiResponse<string>>(genericWrapper.Result);

        // Verify that generic wrapper can also be used as base interface
        IHttpResponseWrapper baseInterfaceView = (HttpResponseWrapper<string>)genericWrapper;
        Assert.True(baseInterfaceView.IsSuccess);
        // Note: baseInterfaceView.Result will be null due to the 'new' keyword hiding the base property
        // This is expected behavior when using the 'new' keyword for property hiding
    }
}
