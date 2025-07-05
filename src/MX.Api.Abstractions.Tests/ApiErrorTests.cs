namespace MX.Api.Abstractions.Tests;

public class ApiErrorTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var error = new ApiError();

        // Assert
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
        Assert.Null(error.Target);
        Assert.Null(error.Details);
    }

    [Fact]
    public void Constructor_WithCodeAndMessage_SetsCodeAndMessage()
    {
        // Arrange
        var code = "NotFound";
        var message = "Resource not found";

        // Act
        var error = new ApiError(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Null(error.Target);
        Assert.Null(error.Details);
    }

    [Fact]
    public void Constructor_WithCodeMessageAndTarget_SetsCodeMessageAndTarget()
    {
        // Arrange
        var code = "ValidationError";
        var message = "Invalid input";
        var target = "email";

        // Act
        var error = new ApiError(code, message, target);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(target, error.Target);
        Assert.Null(error.Details);
    }

    [Fact]
    public void DetailsProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var error = new ApiError("MainError", "Main error occurred");
        var details = new[]
        {
            new ApiError("DetailError1", "Detail error 1 occurred"),
            new ApiError("DetailError2", "Detail error 2 occurred")
        };

        // Act
        error.Details = details;

        // Assert
        Assert.NotNull(error.Details);
        Assert.Equal(2, error.Details.Length);
        Assert.Equal("DetailError1", error.Details[0].Code);
        Assert.Equal("Detail error 1 occurred", error.Details[0].Message);
        Assert.Equal("DetailError2", error.Details[1].Code);
        Assert.Equal("Detail error 2 occurred", error.Details[1].Message);
    }
}
