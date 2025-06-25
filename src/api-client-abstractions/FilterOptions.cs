using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions;

/// <summary>
/// Represents filter options for API queries.
/// </summary>
public class FilterOptions
{
    /// <summary>
    /// Gets or sets the filter expression in OData-like syntax.
    /// </summary>
    [JsonProperty(PropertyName = "filterExpression")]
    public string? FilterExpression { get; set; }

    /// <summary>
    /// Gets or sets the fields to select.
    /// </summary>
    [JsonProperty(PropertyName = "select")]
    public string[]? Select { get; set; }

    /// <summary>
    /// Gets or sets the related entities to expand.
    /// </summary>
    [JsonProperty(PropertyName = "expand")]
    public string[]? Expand { get; set; }

    /// <summary>
    /// Gets or sets the order by expression.
    /// </summary>
    [JsonProperty(PropertyName = "orderBy")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the number of records to skip.
    /// </summary>
    [JsonProperty(PropertyName = "skip")]
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the number of records to take.
    /// </summary>
    [JsonProperty(PropertyName = "top")]
    public int Top { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return only the count of matching records.
    /// </summary>
    [JsonProperty(PropertyName = "count")]
    public bool Count { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterOptions"/> class.
    /// </summary>
    [JsonConstructor]
    public FilterOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterOptions"/> class with default pagination values.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="top">The number of records to take.</param>
    public FilterOptions(int skip = 0, int top = 10)
    {
        Skip = skip;
        Top = top;
    }

    /// <summary>
    /// Creates filter options from query parameter values.
    /// </summary>
    /// <param name="filter">The OData-like filter expression.</param>
    /// <param name="select">Comma-separated list of fields to select.</param>
    /// <param name="expand">Comma-separated list of related entities to expand.</param>
    /// <param name="orderBy">The order by expression.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="top">The number of records to take.</param>
    /// <param name="count">Whether to return only the count of matching records.</param>
    /// <returns>A new instance of <see cref="FilterOptions"/>.</returns>
    public static FilterOptions FromQueryParameters(
        string? filter = null,
        string? select = null,
        string? expand = null,
        string? orderBy = null,
        int skip = 0,
        int top = 10,
        bool count = false)
    {
        return new FilterOptions
        {
            FilterExpression = filter,
            Select = !string.IsNullOrEmpty(select) ? select.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : null,
            Expand = !string.IsNullOrEmpty(expand) ? expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : null,
            OrderBy = orderBy,
            Skip = skip,
            Top = top,
            Count = count
        };
    }
}
