using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model.Results;
using S1.Services;

namespace S1.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: /api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.MsUser
                .FirstOrDefaultAsync(x => x.Username == req.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            var response = new ApiResults<LoginResult>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = new LoginResult
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    IdUser = user.IdUser,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role
                }
            };

            return Ok(response);
        }
    }
}
