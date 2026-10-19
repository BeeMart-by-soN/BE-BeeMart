using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S1.Data;
using S1.Model;
using S1.Model.Results;

namespace S1.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        { _context = context; }

        // GET: /api/<StudentController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetUserResult>>> Get()
        {
            var users = await _context.User
                        .Select(x => new GetUserResult()
                        {
                            Username = x.Username,
                            Fullname = x.Fullname,
                        }).ToListAsync();

            var response = new ApiResults<IEnumerable<GetUserResult>>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = users
            };

            return Ok(response);
        }

        //GET: /api/<StudentController>/:id
        [HttpGet("{id}")]
        public async Task<ActionResult<GetUserResult>> Get(string id)
        {
            var users = await _context.User
                        .Where(x => x.UserID == id)
                        .Select(x => new GetUserResult()
                        {
                            UserID = x.UserID,
                            Username = x.Username,
                            Fullname = x.Fullname,
                        }).FirstOrDefaultAsync();
            
            if(users == null)
            {
                return NotFound("User not found :/");
            }

            var response = new ApiResults<GetUserResult>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = users
            };
            return Ok(response);
        }

        //POST: /api/<StudentController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateUserRequest req)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUsernameTaken = await _context.User.AnyAsync(x => x.Username == req.Username);

            if (isUsernameTaken)
            {
                return BadRequest("Username already exists.");
            }

            if (req.Password != req.RePassword)
            {
                return BadRequest("Password and Re-Password is not same :/");
            }

            var user = new User
            {
                UserID = await GenerateUserId(),
                Username = req.Username,
                Fullname = req.Fullname,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role,
                Datein = DateTime.Now,
                Userin = User?.Identity?.Name ?? "SYSTEM"
            };
            
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<string> GenerateUserId()
        {
            string userId;
            do
            {
                userId = Guid.NewGuid()
                        .ToString("N")[..5]
                        .ToUpper();
            } while (await _context.User.AnyAsync(x => x.UserID == userId));

            return userId;
        }
    }
}

