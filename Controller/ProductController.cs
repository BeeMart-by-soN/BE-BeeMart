using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model.Results;

namespace S1.Controller;

[Authorize]
[Route("api/v1/products")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/v1/products/search?keyword= — maks 10 hasil, LIKE %keyword%, Stsrc=1
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GetProductResult>>>> Search([FromQuery] string? keyword)
    {
        var query = _context.MsProduct
            .Where(x => x.Stsrc);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            // LIKE %keyword% case-insensitive (SQL Server default collation umumnya CI)
            var kw = keyword.Trim();
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{kw}%"));
        }

        var products = await query
            .OrderBy(x => x.Name)
            .Take(10) // maksimal 10 hasil
            .Select(x => new GetProductResult
            {
                IdProduct = x.IdProduct,
                Name = x.Name,
                Weight = x.Weight,
                Stock = x.Stock,
                Price = x.Price,
                Discount = x.Discount
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<GetProductResult>>.Ok(products));
    }

    // GET: /api/v1/products/{id} — detail satu produk
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetProductResult>>> GetById(string id)
    {
        var product = await _context.MsProduct
            .Where(x => x.IdProduct == id && x.Stsrc)
            .Select(x => new GetProductResult
            {
                IdProduct = x.IdProduct,
                Name = x.Name,
                Weight = x.Weight,
                Stock = x.Stock,
                Price = x.Price,
                Discount = x.Discount
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound(ApiResponse<GetProductResult>.Fail("Product not found"));
        }

        return Ok(ApiResponse<GetProductResult>.Ok(product));
    }
}
