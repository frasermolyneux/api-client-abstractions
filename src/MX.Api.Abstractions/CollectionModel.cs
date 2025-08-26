using Newtonsoft.Json;

namespace MX.Api.Abstractions;

/// <summary>
/// Represents a collection of items with additional metadata.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public record CollectionModel<T>
{
    /// <summary>
    /// Gets or sets the collection of items.
    /// </summary>
    [JsonProperty(PropertyName = "items")]
    public IEnumerable<T>? Items { get; set; }

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
    /// <param name="items">The collection of items.</param>
    public CollectionModel(IEnumerable<T>? items)
    {
        Items = items;
    }
}
