namespace MX.Api.IntegrationTests.DummyApis.ProductApi.Models;

/// <summary>
/// Product model for testing
/// </summary>
public class Product
{
    /// <summary>
    /// Product ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Whether the product is in stock
    /// </summary>
    public bool InStock { get; set; }
}
