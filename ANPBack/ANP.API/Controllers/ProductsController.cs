using ANP.API.Data;
using ANP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ANP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Gets all products, ordered by code.
    /// </summary>
    /// <returns>A list of products.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll() =>
        await db.Products.OrderBy(p => p.Code).AsNoTracking().ToListAsync();

    /// <summary>
    /// Gets a product by its code.
    /// </summary>
    /// <param name="code">The code of the product to retrieve.</param>
    /// <returns>The product data or a not-found response.</returns>
    [HttpGet("{code}")]
    public async Task<ActionResult<Product>> GetByCode(string code)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower());

        return product is null ? NotFound() : product;
    }
}
