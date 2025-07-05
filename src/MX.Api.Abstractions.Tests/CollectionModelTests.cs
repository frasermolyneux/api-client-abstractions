namespace MX.Api.Abstractions.Tests;

public class CollectionModelTests
{
    [Fact]
    public void DefaultConstructor_SetsPropertiesToDefaults()
    {
        // Act
        var collection = new CollectionModel<string>();

        // Assert
        Assert.Null(collection.Items);
        Assert.Equal(0, collection.TotalCount);
        Assert.Equal(0, collection.FilteredCount);
        Assert.Null(collection.Metadata);
    }

    [Fact]
    public void Constructor_WithParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var totalCount = 100;
        var filteredCount = 50;

        // Act
        var collection = new CollectionModel<string>(items, totalCount, filteredCount);

        // Assert
        Assert.Equal(items, collection.Items);
        Assert.Equal(totalCount, collection.TotalCount);
        Assert.Equal(filteredCount, collection.FilteredCount);
        Assert.Null(collection.Metadata);
    }

    [Fact]
    public void Constructor_WithNullItems_AcceptsNullItems()
    {
        // Arrange
        List<string>? items = null;
        var totalCount = 100;
        var filteredCount = 50;

        // Act
        var collection = new CollectionModel<string>(items, totalCount, filteredCount);

        // Assert
        Assert.Null(collection.Items);
        Assert.Equal(totalCount, collection.TotalCount);
        Assert.Equal(filteredCount, collection.FilteredCount);
    }

    [Fact]
    public void Constructor_WithEmptyItems_AcceptsEmptyItems()
    {
        // Arrange
        var items = Enumerable.Empty<string>();
        var totalCount = 100;
        var filteredCount = 50;

        // Act
        var collection = new CollectionModel<string>(items, totalCount, filteredCount);

        // Assert
        Assert.Empty(collection.Items!);
        Assert.Equal(totalCount, collection.TotalCount);
        Assert.Equal(filteredCount, collection.FilteredCount);
    }

    [Fact]
    public void Metadata_CanBeSetAndRetrieved()
    {
        // Arrange
        var collection = new CollectionModel<string>();
        var metadata = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        collection.Metadata = metadata;

        // Assert
        Assert.NotNull(collection.Metadata);
        Assert.Equal(2, collection.Metadata.Count);
        Assert.Equal("value1", collection.Metadata["key1"]);
        Assert.Equal("value2", collection.Metadata["key2"]);
    }

    [Fact]
    public void Constructor_WithDifferentItemTypes_WorksCorrectly()
    {
        // Arrange
        var complexItems = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Item 1" },
            new TestItem { Id = 2, Name = "Item 2" }
        };
        var totalCount = 10;
        var filteredCount = 2;

        // Act
        var collection = new CollectionModel<TestItem>(complexItems, totalCount, filteredCount);

        // Assert
        Assert.Equal(complexItems, collection.Items);
        Assert.Equal(totalCount, collection.TotalCount);
        Assert.Equal(filteredCount, collection.FilteredCount);
        Assert.Equal(2, collection.Items!.Count());
        Assert.Equal("Item 1", collection.Items!.First().Name);
    }

    // Test helper class
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
