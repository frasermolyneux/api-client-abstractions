using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions;

/// <summary>
/// A data transfer object representing a collection of items with pagination information.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public record CollectionDto<T>
{
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
