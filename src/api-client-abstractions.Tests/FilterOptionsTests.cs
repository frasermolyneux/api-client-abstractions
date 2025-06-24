namespace MxIO.ApiClient.Abstractions.V2.Tests;

public class FilterOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var filterOptions = new FilterOptions();

        // Assert
        Assert.Null(filterOptions.FilterExpression);
        Assert.Null(filterOptions.Select);
        Assert.Null(filterOptions.Expand);
        Assert.Null(filterOptions.OrderBy);
        Assert.Equal(0, filterOptions.Skip);
        Assert.Equal(0, filterOptions.Top);
        Assert.False(filterOptions.Count);
    }

    [Fact]
    public void Constructor_WithSkipAndTop_SetsSkipAndTopCorrectly()
    {
        // Arrange
        var skip = 20;
        var top = 50;

        // Act
        var filterOptions = new FilterOptions(skip, top);

        // Assert
        Assert.Null(filterOptions.FilterExpression);
        Assert.Null(filterOptions.Select);
        Assert.Null(filterOptions.Expand);
        Assert.Null(filterOptions.OrderBy);
        Assert.Equal(skip, filterOptions.Skip);
        Assert.Equal(top, filterOptions.Top);
        Assert.False(filterOptions.Count);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_SetsDefaultValues()
    {
        // Act
        var filterOptions = new FilterOptions();

        // Assert
        Assert.Equal(0, filterOptions.Skip);
        Assert.Equal(0, filterOptions.Top);
    }

    [Fact]
    public void FromQueryParameters_WithNullValues_ReturnsFilterOptionsWithDefaults()
    {
        // Act
        var filterOptions = FilterOptions.FromQueryParameters();

        // Assert
        Assert.Null(filterOptions.FilterExpression);
        Assert.Null(filterOptions.Select);
        Assert.Null(filterOptions.Expand);
        Assert.Null(filterOptions.OrderBy);
        Assert.Equal(0, filterOptions.Skip);
        Assert.Equal(10, filterOptions.Top); // Default top value is 10
        Assert.False(filterOptions.Count);
    }

    [Fact]
    public void FromQueryParameters_WithAllValues_SetsAllProperties()
    {
        // Arrange
        var filter = "name eq 'John'";
        var select = "name,age,email";
        var expand = "addresses,orders";
        var orderBy = "name asc";
        var skip = 20;
        var top = 50;
        var count = true;

        // Act
        var filterOptions = FilterOptions.FromQueryParameters(
            filter: filter,
            select: select,
            expand: expand,
            orderBy: orderBy,
            skip: skip,
            top: top,
            count: count);

        // Assert
        Assert.Equal(filter, filterOptions.FilterExpression);
        Assert.NotNull(filterOptions.Select);
        Assert.Equal(3, filterOptions.Select.Length);
        Assert.Contains("name", filterOptions.Select);
        Assert.Contains("age", filterOptions.Select);
        Assert.Contains("email", filterOptions.Select);
        Assert.NotNull(filterOptions.Expand);
        Assert.Equal(2, filterOptions.Expand.Length);
        Assert.Contains("addresses", filterOptions.Expand);
        Assert.Contains("orders", filterOptions.Expand);
        Assert.Equal(orderBy, filterOptions.OrderBy);
        Assert.Equal(skip, filterOptions.Skip);
        Assert.Equal(top, filterOptions.Top);
        Assert.True(filterOptions.Count);
    }

    [Fact]
    public void FromQueryParameters_WithEmptyStrings_ReturnsNullArrays()
    {
        // Arrange
        var select = "";
        var expand = "";

        // Act
        var filterOptions = FilterOptions.FromQueryParameters(
            select: select,
            expand: expand);

        // Assert
        Assert.Null(filterOptions.Select);
        Assert.Null(filterOptions.Expand);
    }

    [Fact]
    public void FromQueryParameters_WithSpaces_TrimsEntries()
    {
        // Arrange
        var select = "name, age , email";
        var expand = " addresses , orders ";

        // Act
        var filterOptions = FilterOptions.FromQueryParameters(
            select: select,
            expand: expand);

        // Assert
        Assert.NotNull(filterOptions.Select);
        Assert.Equal(3, filterOptions.Select.Length);
        Assert.Contains("name", filterOptions.Select);
        Assert.Contains("age", filterOptions.Select);
        Assert.Contains("email", filterOptions.Select);
        Assert.NotNull(filterOptions.Expand);
        Assert.Equal(2, filterOptions.Expand.Length);
        Assert.Contains("addresses", filterOptions.Expand);
        Assert.Contains("orders", filterOptions.Expand);
    }

    [Fact]
    public void FromQueryParameters_WithEmptyEntries_RemovesEmptyEntries()
    {
        // Arrange
        var select = "name,,age,,email";
        var expand = ",addresses,,orders,";

        // Act
        var filterOptions = FilterOptions.FromQueryParameters(
            select: select,
            expand: expand);

        // Assert
        Assert.NotNull(filterOptions.Select);
        Assert.Equal(3, filterOptions.Select.Length);
        Assert.Contains("name", filterOptions.Select);
        Assert.Contains("age", filterOptions.Select);
        Assert.Contains("email", filterOptions.Select);
        Assert.NotNull(filterOptions.Expand);
        Assert.Equal(2, filterOptions.Expand.Length);
        Assert.Contains("addresses", filterOptions.Expand);
        Assert.Contains("orders", filterOptions.Expand);
    }
}
