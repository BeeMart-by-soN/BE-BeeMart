using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model;
using S1.Model.Results;

namespace S1.Controller
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        { _context = context; }

        // GET: /api/<ProcuctController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetProductResults>>> Get()
        {
            var products = await _context.Product
                        .Select(x => new GetProductResults()
                        {
                            Name = x.Name,
                            Weight = x.Weight,
                            Stock = x.Stock,
                            Price = x.Price,
                            Discount = x.Discount
                        }).ToListAsync();

            var response = new ApiResults<IEnumerable<GetProductResults>>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = products
            };

            return Ok(response);
        }
    }
}