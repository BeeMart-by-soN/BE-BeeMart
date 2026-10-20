using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model.Results;

namespace S1.Controller;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/v1/User — daftar user (hanya Admin)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<GetUserResult>>>> Get()
    {
        var users = await _context.MsUser
            .Where(x => x.Stsrc)
            .Select(x => new GetUserResult
            {
                UserId = x.IdUser,
                Username = x.Username,
                Fullname = x.FullName
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<GetUserResult>>.Ok(users));
    }

    // GET: /api/v1/User/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetUserResult>>> Get(string id)
    {
        var user = await _context.MsUser
            .Where(x => x.IdUser == id && x.Stsrc)
            .Select(x => new GetUserResult
            {
                UserId = x.IdUser,
                Username = x.Username,
                Fullname = x.FullName
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(ApiResponse<GetUserResult>.Fail("User not found"));
        }

        return Ok(ApiResponse<GetUserResult>.Ok(user));
    }
}
