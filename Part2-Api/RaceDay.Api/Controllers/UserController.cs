using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]   // every endpoint in this class needs a logged-in user
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        // Reads the user id that Login stored in the cookie
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            if (user == null) return Unauthorized();

            // An anonymous object, so PasswordHash is never sent back
            return Ok(new { user.UserId, user.FullName, user.Email, Role = user.Role.RoleName, user.CreatedAt });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UpdateProfileDto dto)
        {
            var user = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            if (user == null) return Unauthorized();

            // Email must stay unique, but you may keep your own
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != user.UserId))
                return Conflict("Email already in use.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            await _db.SaveChangesAsync();

            return Ok(new { user.UserId, user.FullName, user.Email, Role = user.Role.RoleName });
        }
    }
}
