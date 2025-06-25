namespace MxIO.ApiClient.Abstractions.Tests;

public class ApiPaginationTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var pagination = new ApiPagination();

        // Assert
        Assert.Equal(0, pagination.TotalCount);
        Assert.Equal(0, pagination.FilteredCount);
        Assert.Equal(0, pagination.Skip);
        Assert.Equal(0, pagination.Top);
        Assert.False(pagination.HasMore);
    }

    [Theory]
    [InlineData(100, 80, 0, 10, true)]  // More items available
    [InlineData(100, 80, 70, 10, false)] // No more items
    [InlineData(100, 80, 80, 10, false)] // Exactly at the end
    [InlineData(100, 10, 0, 10, false)]  // Exactly enough items
    [InlineData(100, 5, 0, 10, false)]   // Fewer items than requested
    [InlineData(0, 0, 0, 10, false)]     // No items
    public void Constructor_WithParameters_SetsPropertiesCorrectly(int totalCount, int filteredCount, int skip, int top, bool expectedHasMore)
    {
        // Act
        var pagination = new ApiPagination(totalCount, filteredCount, skip, top);

        // Assert
        Assert.Equal(totalCount, pagination.TotalCount);
        Assert.Equal(filteredCount, pagination.FilteredCount);
        Assert.Equal(skip, pagination.Skip);
        Assert.Equal(top, pagination.Top);
        Assert.Equal(expectedHasMore, pagination.HasMore);
    }

    [Fact]
    public void HasMore_CalculatedCorrectly_WhenFilteredCountExceedsSkipPlusTop()
    {
        // Arrange
        var totalCount = 100;
        var filteredCount = 50;
        var skip = 0;
        var top = 10;

        // Act
        var pagination = new ApiPagination(totalCount, filteredCount, skip, top);

        // Assert
        Assert.True(pagination.HasMore);
        Assert.Equal(40, filteredCount - (skip + top)); // 50 - (0 + 10) = 40 more items
    }

    [Fact]
    public void HasMore_CalculatedCorrectly_WhenFilteredCountEqualsSkipPlusTop()
    {
        // Arrange
        var totalCount = 100;
        var filteredCount = 30;
        var skip = 10;
        var top = 20;

        // Act
        var pagination = new ApiPagination(totalCount, filteredCount, skip, top);

        // Assert
        Assert.False(pagination.HasMore);
        Assert.Equal(0, filteredCount - (skip + top)); // 30 - (10 + 20) = 0 more items
    }

    [Fact]
    public void HasMore_CalculatedCorrectly_WhenFilteredCountLessThanSkipPlusTop()
    {
        // Arrange
        var totalCount = 100;
        var filteredCount = 25;
        var skip = 10;
        var top = 20;

        // Act
        var pagination = new ApiPagination(totalCount, filteredCount, skip, top);

        // Assert
        Assert.False(pagination.HasMore);
        Assert.Equal(-5, filteredCount - (skip + top)); // 25 - (10 + 20) = -5 (no more items)
    }
}
