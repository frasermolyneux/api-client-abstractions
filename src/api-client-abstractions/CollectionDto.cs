using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions;

/// <summary>
/// Interface for a data transfer object representing a collection of items with pagination information.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public interface ICollectionDto<T>
{
    /// <summary>
    /// Gets or sets the total number of records available.
    /// </summary>
    int TotalRecords { get; }

    /// <summary>
    /// Gets or sets the number of records after applying any filters.
    /// </summary>
    int FilteredRecords { get; }

    /// <summary>
    /// Gets or sets the collection of items.
    /// </summary>
    List<T> Entries { get; }
}

/// <summary>
/// A data transfer object representing a collection of items with pagination information.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public record CollectionDto<T> : ICollectionDto<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDto{T}"/> class.
    /// </summary>
    [JsonConstructor]
    public CollectionDto()
    {
        Entries = new List<T>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDto{T}"/> class with the specified values.
    /// </summary>
    /// <param name="totalRecords">The total number of records available.</param>
    /// <param name="filteredRecords">The number of records after applying any filters.</param>
    /// <param name="entries">The collection of items.</param>
    public CollectionDto(int totalRecords, int filteredRecords, IEnumerable<T>? entries = null)
    {
        TotalRecords = totalRecords < 0 ? 0 : totalRecords;
        FilteredRecords = filteredRecords < 0 ? 0 : filteredRecords;
        Entries = entries?.ToList() ?? new List<T>();
    }

    /// <summary>
    /// Gets or sets the total number of records available.
    /// </summary>
    [JsonProperty(PropertyName = "totalRecords")]
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of records after applying any filters.
    /// </summary>
    [JsonProperty(PropertyName = "filteredRecords")]
    public int FilteredRecords { get; set; }

    /// <summary>
    /// Gets or sets the collection of items.
    /// </summary>
    [JsonProperty(PropertyName = "entries")]
    public List<T> Entries { get; set; } = new();
}
