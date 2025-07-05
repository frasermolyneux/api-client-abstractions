using Newtonsoft.Json;

namespace MX.Api.Abstractions;

/// <summary>
/// Represents pagination information in an API response.
/// </summary>
public class ApiPagination
{
    /// <summary>
    /// Gets or sets the total count of records available.
    /// </summary>
    [JsonProperty(PropertyName = "totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the count of records after filtering.
    /// </summary>
    [JsonProperty(PropertyName = "filteredCount")]
    public int FilteredCount { get; set; }

    /// <summary>
    /// Gets or sets the number of records skipped.
    /// </summary>
    [JsonProperty(PropertyName = "skip")]
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the number of records to take.
    /// </summary>
    [JsonProperty(PropertyName = "top")]
    public int Top { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there are more records available.
    /// </summary>
    [JsonProperty(PropertyName = "hasMore")]
    public bool HasMore { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiPagination"/> class.
    /// </summary>
    [JsonConstructor]
    public ApiPagination()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiPagination"/> class with specified parameters.
    /// </summary>
    /// <param name="totalCount">The total count of records available.</param>
    /// <param name="filteredCount">The count of records after filtering.</param>
    /// <param name="skip">The number of records skipped.</param>
    /// <param name="top">The number of records to take.</param>
    public ApiPagination(int totalCount, int filteredCount, int skip, int top)
    {
        TotalCount = totalCount;
        FilteredCount = filteredCount;
        Skip = skip;
        Top = top;
        HasMore = filteredCount > skip + top;
    }
}
