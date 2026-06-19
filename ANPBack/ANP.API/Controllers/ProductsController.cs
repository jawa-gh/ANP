using ANP.API.Data;
using ANP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ANP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    // GET /api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll() =>
        await db.Products.OrderBy(p => p.Code).ToListAsync();

    // GET /api/products/ACME-001
    // Used by the Angular `validateHttp` async validator: 200 => valid code, 404 => unknown.
    [HttpGet("{code}")]
    public async Task<ActionResult<Product>> GetByCode(string code)
    {
        var product = await db.Products.FirstOrDefaultAsync(p =>
            p.Code.ToLower() == code.ToLower()
        );
        return product is null ? NotFound() : product;
    }
}
