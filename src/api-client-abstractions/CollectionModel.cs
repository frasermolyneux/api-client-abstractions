using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions;

/// <summary>
/// Represents a collection of items with additional metadata.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class CollectionModel<T>
{
    /// <summary>
    /// Gets or sets the collection of items.
    /// </summary>
    [JsonProperty(PropertyName = "items")]
    public IEnumerable<T>? Items { get; set; }

    /// <summary>
    /// Gets or sets the total count of items (before pagination).
    /// </summary>
    [JsonProperty(PropertyName = "totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the count of items after filtering.
    /// </summary>
    [JsonProperty(PropertyName = "filteredCount")]
    public int FilteredCount { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the collection.
    /// </summary>
    [JsonProperty(PropertyName = "metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionModel{T}"/> class.
    /// </summary>
    [JsonConstructor]
    public CollectionModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionModel{T}"/> class with the specified items.
    /// </summary>
    /// <param name="items">The collection of items.</param>
    /// <param name="totalCount">The total count of items.</param>
    /// <param name="filteredCount">The count of items after filtering.</param>
    public CollectionModel(IEnumerable<T>? items, int totalCount, int filteredCount)
    {
        Items = items;
        TotalCount = totalCount;
        FilteredCount = filteredCount;
    }
}
