using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model.Results;

namespace S1.Controller;

[Authorize]
[Route("api/v1/payment-methods")]
[ApiController]
public class PaymentMethodController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentMethodController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/v1/payment-methods — daftar metode pembayaran aktif
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<GetPaymentMethodResult>>>> Get()
    {
        var methods = await _context.LtPaymentMethod
            .Where(x => x.Stsrc)
            .OrderBy(x => x.IdPaymentMethod)
            .Select(x => new GetPaymentMethodResult
            {
                IdPaymentMethod = x.IdPaymentMethod,
                MethodName = x.MethodName
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<GetPaymentMethodResult>>.Ok(methods));
    }
}
