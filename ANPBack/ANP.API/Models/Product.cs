using System.ComponentModel.DataAnnotations;

namespace ANP.API.Models;

/// <summary>
/// Catalog product. The <see cref="Code"/> is the natural key the frontend's
/// async Signal Forms validator (<c>validateHttp</c>) checks against.
/// </summary>
public class Product
{
    [Key]
    public required string Code { get; set; }

    public required string Name { get; set; }

    public required decimal DefaultPrice { get; set; }
}
