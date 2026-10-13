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
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(AppDbContext db) { _db = db; }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Only these two roles are allowed, so nobody can invent an "Admin"
            if (dto.Role != "Organizer" && dto.Role != "Participant")
                return BadRequest("Role must be Organizer or Participant.");

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already registered.");

            var role = await _db.Roles.FirstAsync(r => r.RoleName == dto.Role);

            var user = new User { FullName = dto.FullName, Email = dto.Email, RoleId = role.RoleId };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);   // never store the plain password

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { user.UserId, Role = role.RoleName });
        }

        [HttpPost("login")]
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
                new Claim(ClaimTypes.Role, user.Role.RoleName)   // what [Authorize(Roles = "...")] reads
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));   // creates the session cookie

            return Ok(new { user.UserId, Role = user.Role.RoleName });
        }
    }
}
