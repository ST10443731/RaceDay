using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Tags("Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(AppDbContext db) { _db = db; }

        /// <summary>
        /// Registers a new user as an Organizer or a Participant.
        /// </summary>
        /// <remarks>
        /// Expects fullName, email, password (at least 6 characters) and role ("Organizer" or "Participant").
        /// The password is hashed before it is stored.
        /// </remarks>
        /// <response code="201">User created. Returns the new user id and role.</response>
        /// <response code="400">Validation failed, or the role is not Organizer or Participant.</response>
        /// <response code="409">The email is already registered.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Role != "Organizer" && dto.Role != "Participant")
                return BadRequest("Role must be Organizer or Participant.");

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already registered.");

            var role = await _db.Roles.FirstAsync(r => r.RoleName == dto.Role);

            var user = new User { FullName = dto.FullName, Email = dto.Email, RoleId = role.RoleId };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { user.UserId, Role = role.RoleName });
        }

        /// <summary>
        /// Logs a user in and starts a session.
        /// </summary>
        /// <remarks>
        /// Expects email and password. On success a session cookie is set, which identifies the
        /// user and their role on every later request.
        /// </remarks>
        /// <response code="200">Login successful. Returns the user id and role.</response>
        /// <response code="401">Invalid email or password.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null) return Unauthorized("Invalid credentials.");

            var check = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (check == PasswordVerificationResult.Failed) return Unauthorized("Invalid credentials.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return Ok(new { user.UserId, Role = user.Role.RoleName });
        }
    }
}