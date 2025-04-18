using MxIO.ApiClient.Abstractions;
using Xunit;

namespace MxIO.ApiClient.Abstractions.Tests;

public class CollectionDtoTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyEntries()
    {
        // Arrange & Act
        var collection = new CollectionDto<string>();

        // Assert
        Assert.NotNull(collection.Entries);
        Assert.Empty(collection.Entries);
        Assert.Equal(0, collection.TotalRecords);
        Assert.Equal(0, collection.FilteredRecords);
    }

    [Fact]
    public void Constructor_WithTotalAndFilteredRecords_ShouldSetPropertiesAndEmptyEntries()
    {
        // Arrange & Act
        var totalRecords = 100;
        var filteredRecords = 50;
        var collection = new CollectionDto<string>(totalRecords, filteredRecords);

        // Assert
        Assert.NotNull(collection.Entries);
        Assert.Empty(collection.Entries);
        Assert.Equal(totalRecords, collection.TotalRecords);
        Assert.Equal(filteredRecords, collection.FilteredRecords);
    }

    [Fact]
    public void Constructor_WithTotalFilteredAndEntries_ShouldSetAllProperties()
    {
        // Arrange
        var totalRecords = 100;
        var filteredRecords = 50;
        var entries = new List<string> { "Item1", "Item2", "Item3" };

        // Act
        var collection = new CollectionDto<string>(totalRecords, filteredRecords, entries);

        // Assert
        Assert.NotNull(collection.Entries);
        Assert.Equal(3, collection.Entries.Count);
        Assert.Equal(totalRecords, collection.TotalRecords);
        Assert.Equal(filteredRecords, collection.FilteredRecords);
        Assert.Equal(entries, collection.Entries);
    }

    [Fact]
    public void Constructor_WithNullEntries_ShouldCreateEmptyList()
    {
        // Arrange & Act
        var collection = new CollectionDto<string>(100, 50, null);

        // Assert
        Assert.NotNull(collection.Entries);
        Assert.Empty(collection.Entries);
    }

    [Fact]
    public void SetProperties_ShouldPersistChanges()
    {
        // Arrange
        var collection = new CollectionDto<string>();

        // Act
        collection.TotalRecords = 200;
        collection.FilteredRecords = 100;
        collection.Entries = new List<string> { "UpdatedItem1", "UpdatedItem2" };

        // Assert
        Assert.Equal(200, collection.TotalRecords);
        Assert.Equal(100, collection.FilteredRecords);
        Assert.Equal(2, collection.Entries.Count);
        Assert.Equal("UpdatedItem1", collection.Entries[0]);
        Assert.Equal("UpdatedItem2", collection.Entries[1]);
    }
}