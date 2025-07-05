using System.Net;

namespace MX.Api.Abstractions.Tests;

public class ApiResultTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var wrapper = new ApiResult();

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
        var wrapper = new ApiResult(statusCode);

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
        var wrapper = new ApiResult(statusCode, apiResponse);

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
        var wrapper = new ApiResult(statusCode, apiResponse);

        // Assert
        Assert.Equal(statusCode, wrapper.StatusCode);
        Assert.Null(wrapper.Result);
        Assert.True(wrapper.IsSuccess); // IsSuccess in base class only depends on status code
    }

    [Fact]
    public void ApiResult_Implementation_ImplementsInterface()
    {
        // Act
        var wrapper = new ApiResult();

        // Assert
        Assert.IsAssignableFrom<IApiResult>(wrapper);
    }

    [Fact]
    public void ApiResult_ResultProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var wrapper = new ApiResult();
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
    public void ApiResult_IsSuccess_DependsOnlyOnStatusCode(HttpStatusCode statusCode, bool expectedIsSuccess)
    {
        // Arrange
        var wrapperWithNullResult = new ApiResult(statusCode);
        var wrapperWithResult = new ApiResult(statusCode, new ApiResponse());

        // Assert - Both should have the same IsSuccess value regardless of Result
        Assert.Equal(expectedIsSuccess, wrapperWithNullResult.IsSuccess);
        Assert.Equal(expectedIsSuccess, wrapperWithResult.IsSuccess);
    }

    [Fact]
    public void GenericApiResult_DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var wrapper = new ApiResult<string>();

        // Assert
        Assert.Equal(default(HttpStatusCode), wrapper.StatusCode);
        Assert.False(wrapper.IsSuccess);
        Assert.Null(wrapper.Result);
    }

    [Fact]
    public void GenericApiResult_ConstructorWithStatusCode_SetsStatusCode()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act
        var wrapper = new ApiResult<string>(statusCode);

        // Assert
        Assert.Equal(statusCode, wrapper.StatusCode);
        Assert.False(wrapper.IsSuccess); // IsSuccess is overridden in generic class to require a non-null Result
        Assert.Null(wrapper.Result);
    }

    [Fact]
    public void GenericApiResult_ConstructorWithStatusCodeAndResult_SetsStatusCodeAndResult()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var apiResponse = new ApiResponse<string>("Test data");

        // Act
        var wrapper = new ApiResult<string>(statusCode, apiResponse);

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
    public void GenericApiResult_IsSuccess_RequiresBothSuccessStatusCodeAndNonNullResult(HttpStatusCode statusCode, bool baseIsSuccess)
    {
        // Arrange
        var wrapperWithNullResult = new ApiResult<string>(statusCode);
        var wrapperWithResult = new ApiResult<string>(statusCode, new ApiResponse<string>());

        // Assert
        Assert.False(wrapperWithNullResult.IsSuccess);
        Assert.Equal(baseIsSuccess, wrapperWithResult.IsSuccess);
    }

    [Fact]
    public void GenericApiResult_Implementation_ImplementsInterface()
    {
        // Act
        var wrapper = new ApiResult<string>();

        // Assert
        Assert.IsAssignableFrom<IApiResult>(wrapper);
        Assert.IsAssignableFrom<IApiResult<string>>(wrapper);
    }

    [Fact]
    public void GenericApiResult_InheritsFromApiResult()
    {
        // Act
        var wrapper = new ApiResult<string>();

        // Assert
        Assert.IsAssignableFrom<ApiResult>(wrapper);
        Assert.IsAssignableFrom<IApiResult>(wrapper);
        Assert.IsAssignableFrom<IApiResult<string>>(wrapper);
    }

    [Fact]
    public void ApiResult_And_GenericApiResult_CanBeUsedPolymorphically()
    {
        // Arrange
        IApiResult baseWrapper = new ApiResult(HttpStatusCode.OK, new ApiResponse());
        IApiResult<string> genericWrapper = new ApiResult<string>(HttpStatusCode.OK, new ApiResponse<string>("test"));

        // Assert
        Assert.True(baseWrapper.IsSuccess);
        Assert.True(genericWrapper.IsSuccess);
        Assert.NotNull(baseWrapper.Result);
        Assert.NotNull(genericWrapper.Result);

        // Verify types
        Assert.IsType<ApiResponse>(baseWrapper.Result);
        Assert.IsType<ApiResponse<string>>(genericWrapper.Result);

        // Verify that generic wrapper can also be used as base interface
        IApiResult baseInterfaceView = (ApiResult<string>)genericWrapper;
        Assert.True(baseInterfaceView.IsSuccess);
        // Note: baseInterfaceView.Result will be null due to the 'new' keyword hiding the base property
        // This is expected behavior when using the 'new' keyword for property hiding
    }
}
