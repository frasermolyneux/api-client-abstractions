using MxIO.ApiClient.Abstractions;
using Newtonsoft.Json;
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

    [Fact]
    public void Constructor_WithNegativeTotalRecords_ShouldDefaultToZero()
    {
        // Arrange & Act
        var totalRecords = -10;
        var filteredRecords = 5;
        var collection = new CollectionDto<string>(totalRecords, filteredRecords);

        // Assert
        Assert.Equal(0, collection.TotalRecords);
        Assert.Equal(filteredRecords, collection.FilteredRecords);
    }

    [Fact]
    public void Constructor_WithNegativeFilteredRecords_ShouldDefaultToZero()
    {
        // Arrange & Act
        var totalRecords = 10;
        var filteredRecords = -5;
        var collection = new CollectionDto<string>(totalRecords, filteredRecords);

        // Assert
        Assert.Equal(totalRecords, collection.TotalRecords);
        Assert.Equal(0, collection.FilteredRecords);
    }

    [Fact]
    public void Collections_WithDifferentTypes_ShouldWorkWithGenericConstraints()
    {
        // Complex type collection
        var complexCollection = new CollectionDto<TestType>();
        complexCollection.Entries.Add(new TestType { Id = 1, Name = "Test" });

        // Value type collection
        var intCollection = new CollectionDto<int>();
        intCollection.Entries.Add(42);

        // Nullable type collection
        var nullableCollection = new CollectionDto<int?>();
        nullableCollection.Entries.Add(null);

        // Assert
        Assert.Single(complexCollection.Entries);
        Assert.Equal(1, complexCollection.Entries[0].Id);
        Assert.Single(intCollection.Entries);
        Assert.Equal(42, intCollection.Entries[0]);
        Assert.Single(nullableCollection.Entries);
        Assert.Null(nullableCollection.Entries[0]);
    }

    [Fact]
    public void Constructor_WithFilteredRecordsLargerThanTotalRecords_ShouldKeepOriginalValues()
    {
        // Arrange & Act
        var totalRecords = 10;
        var filteredRecords = 20; // Logically could be invalid but the class doesn't enforce this constraint
        var collection = new CollectionDto<string>(totalRecords, filteredRecords);

        // Assert
        Assert.Equal(totalRecords, collection.TotalRecords);
        Assert.Equal(filteredRecords, collection.FilteredRecords);
    }

    [Fact]
    public void Constructor_WithEntriesMoreThanFilteredRecords_ShouldKeepOriginalValues()
    {
        // Arrange
        var totalRecords = 100;
        var filteredRecords = 2;
        var entries = new List<string> { "Entry1", "Entry2", "Entry3" }; // More entries than filtered records

        // Act
        var collection = new CollectionDto<string>(totalRecords, filteredRecords, entries);

        // Assert
        Assert.Equal(totalRecords, collection.TotalRecords);
        Assert.Equal(filteredRecords, collection.FilteredRecords);
        Assert.Equal(3, collection.Entries.Count); // Should preserve all entries
    }

    [Fact]
    public void EntriesProperty_ShouldBeModifiable()
    {
        // Arrange
        var collection = new CollectionDto<string>();

        // Act - Add some entries directly to the Entries property
        collection.Entries.Add("Item1");
        collection.Entries.Add("Item2");

        // Act - Remove an entry
        collection.Entries.RemoveAt(0);

        // Assert
        Assert.Single(collection.Entries);
        Assert.Equal("Item2", collection.Entries[0]);
    }

    [Fact]
    public void JsonSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var totalRecords = 100;
        var filteredRecords = 50;
        var entries = new List<string> { "Item1", "Item2", "Item3" };
        var collection = new CollectionDto<string>(totalRecords, filteredRecords, entries);

        // Act
        var json = JsonConvert.SerializeObject(collection);
        var deserializedCollection = JsonConvert.DeserializeObject<CollectionDto<string>>(json);

        // Assert
        Assert.NotNull(deserializedCollection);
        Assert.Equal(totalRecords, deserializedCollection.TotalRecords);
        Assert.Equal(filteredRecords, deserializedCollection.FilteredRecords);
        Assert.Equal(3, deserializedCollection.Entries.Count);
        Assert.Equal("Item1", deserializedCollection.Entries[0]);
        Assert.Equal("Item2", deserializedCollection.Entries[1]);
        Assert.Equal("Item3", deserializedCollection.Entries[2]);
    }

    [Fact]
    public void JsonSerialization_WithComplexType_ShouldPreserveAllProperties()
    {
        // Arrange
        var totalRecords = 100;
        var filteredRecords = 50;
        var entries = new List<TestType>
        {
            new TestType { Id = 1, Name = "First Item" },
            new TestType { Id = 2, Name = "Second Item" }
        };

        var collection = new CollectionDto<TestType>(totalRecords, filteredRecords, entries);

        // Act
        var json = JsonConvert.SerializeObject(collection);
        var deserializedCollection = JsonConvert.DeserializeObject<CollectionDto<TestType>>(json);

        // Assert
        Assert.NotNull(deserializedCollection);
        Assert.Equal(totalRecords, deserializedCollection.TotalRecords);
        Assert.Equal(filteredRecords, deserializedCollection.FilteredRecords);
        Assert.Equal(2, deserializedCollection.Entries.Count);
        Assert.Equal(1, deserializedCollection.Entries[0].Id);
        Assert.Equal("First Item", deserializedCollection.Entries[0].Name);
        Assert.Equal(2, deserializedCollection.Entries[1].Id);
        Assert.Equal("Second Item", deserializedCollection.Entries[1].Name);
    }

    [Fact]
    public void EmptyCollection_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var collection = new CollectionDto<string>(0, 0);

        // Act
        var json = JsonConvert.SerializeObject(collection);
        var deserializedCollection = JsonConvert.DeserializeObject<CollectionDto<string>>(json);

        // Assert
        Assert.NotNull(deserializedCollection);
        Assert.Equal(0, deserializedCollection.TotalRecords);
        Assert.Equal(0, deserializedCollection.FilteredRecords);
        Assert.Empty(deserializedCollection.Entries);
    }

    [Fact]
    public void UpdateProperties_AfterConstruction_ShouldWorkCorrectly()
    {
        // Arrange
        var collection = new CollectionDto<string>(10, 5);

        // Act
        collection.TotalRecords = 20;
        collection.FilteredRecords = 15;
        collection.Entries.Add("New Item");

        // Assert
        Assert.Equal(20, collection.TotalRecords);
        Assert.Equal(15, collection.FilteredRecords);
        Assert.Single(collection.Entries);
        Assert.Equal("New Item", collection.Entries[0]);
    }

    [Fact]
    public void Constructor_WithZeroValues_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var collection = new CollectionDto<string>(0, 0);

        // Assert
        Assert.Equal(0, collection.TotalRecords);
        Assert.Equal(0, collection.FilteredRecords);
        Assert.Empty(collection.Entries);
    }

    // Test class for complex type testing
    private class TestType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}