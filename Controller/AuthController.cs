using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model;
using S1.Model.Requests;
using S1.Model.Results;
using S1.Services;

namespace S1.Controller;

[Route("api/v1/auth")]
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

    // POST: /api/v1/auth/register
    // - Dipanggil anonim dari halaman Login/Register (pendaftaran mandiri).
    // - Dipanggil oleh Admin yang sudah login untuk menambah akun (fitur tambah akun).
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
        {
            var msg = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<object>.Fail(msg));
        }

        // Bila request membawa token, hanya role Admin yang boleh memakai endpoint ini.
        // (Tanpa token = pendaftaran mandiri dari halaman Register.)
        if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail("Hanya Admin yang dapat menambah akun."));
        }

        // Password & Re-Password harus sama (validasi server)
        if (req.Password != req.RePassword)
        {
            return BadRequest(ApiResponse<object>.Fail("Password dan Re-Password tidak cocok"));
        }

        // Role harus valid
        var role = Capitalize(req.Role);
        if (role != "Admin" && role != "Cashier")
        {
            return BadRequest(ApiResponse<object>.Fail("Role tidak valid"));
        }

        // Username harus unik
        var usernameTaken = await _context.MsUser
            .AnyAsync(x => x.Username.ToLower() == req.Username.ToLower());
        if (usernameTaken)
        {
            return Conflict(ApiResponse<object>.Fail("Username already exists"));
        }

        // Buat IdUser 5 digit numerik unik
        var idUser = await GenerateNumericUserIdAsync();

        var user = new MsUser
        {
            IdUser = idUser,
            Username = req.Username.Trim(),
            FullName = req.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 11),
            Role = role,
            Stsrc = true,
            Datein = WibTime.Now,
            Userin = "SYSTEM"
        };

        _context.MsUser.Add(user);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { idUser = user.IdUser }, "Registrasi berhasil! Silakan login."));
    }

    // POST: /api/v1/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResult>>> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid)
        {
            var msg = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<LoginResult>.Fail(msg));
        }

        // Cari user aktif berdasarkan username
        var user = await _context.MsUser
            .FirstOrDefaultAsync(x => x.Username.ToLower() == req.Username.ToLower() && x.Stsrc);

        // Validasi: username + password + role harus semua cocok.
        // Jika ada yang tidak sesuai -> "Invalid login" (jangan bocorkan mana yang salah).
        if (user == null
            || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)
            || !string.Equals(user.Role, Capitalize(req.Role), StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(ApiResponse<LoginResult>.Fail("Invalid login"));
        }

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        var data = new LoginResult
        {
            Token = token,
            ExpiresAt = expiresAt,
            IdUser = user.IdUser,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        };

        return Ok(ApiResponse<LoginResult>.Ok(data, "Login berhasil"));
    }

    // GET: /api/v1/auth/me — info user yang sedang login (dari token)
    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResponse<object>> Me()
    {
        var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var fullName = User.FindFirstValue(ClaimTypes.GivenName);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(ApiResponse<object>.Ok(new { idUser, username, fullName, role }));
    }

    /// <summary>Generate IdUser 5 digit numerik (10000-99999) yang unik.</summary>
    private async Task<string> GenerateNumericUserIdAsync()
    {
        var rng = new Random();
        string idUser;
        do
        {
            idUser = rng.Next(10000, 100000).ToString(); // 5 digit: 10000..99999
        } while (await _context.MsUser.AnyAsync(x => x.IdUser == idUser));
        return idUser;
    }

    private static string Capitalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty
            : char.ToUpper(value.Trim()[0]) + value.Trim()[1..].ToLower();
}
