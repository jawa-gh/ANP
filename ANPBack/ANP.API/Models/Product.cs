namespace ANP.API.Models;

/// <summary>
/// Catalog product. The <see cref="Code"/> is the natural key the frontend's
/// async Signal Forms validator (<c>validateHttp</c>) checks against.
/// </summary>
public class Product
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }
}
