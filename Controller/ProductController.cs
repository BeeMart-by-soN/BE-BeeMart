using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
        public async Task<ActionResult<IEnumerable<GetProductResult>>> Get()
        {
            var products = await _context.MsProduct
                        .Select(x => new GetProductResult()
                        {
                            Name = x.Name,
                            Weight = x.Weight,
                            Stock = x.Stock,
                            Price = x.Price,
                            Discount = x.Discount
                        }).ToListAsync();

            var response = new ApiResults<IEnumerable<GetProductResult>>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = products
            };

            return Ok(response);
        }

        //GET: /api/<controller>/:name
        [HttpGet("{name}")]
        public async Task<ActionResult<GetProductResult>> Get(string name)
        {
            var products = await _context.MsProduct
                            .Where(x => x.Name == name)
                            .Select(x => new GetProductResult()
                            {
                                Name = x.Name,
                                Weight = x.Weight,
                                Stock = x.Stock,
                                Price = x.Price,
                                Discount = x.Discount,
                            }).FirstOrDefaultAsync();

            if (products == null)
            {
                return NotFound("Product not found");
            }
            
            var response = new ApiResults<GetProductResult>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = products
            };
            return Ok();
        }
    }
}
