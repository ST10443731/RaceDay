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
    [Authorize]
    [Tags("User Profile")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Returns the profile of the logged-in user.
        /// </summary>
        /// <remarks>No body needed. Available to both Organizers and Participants.</remarks>
        /// <response code="200">The user's id, name, email, role and creation date.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe()
        {
            var user = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            if (user == null) return Unauthorized();

            return Ok(new { user.UserId, user.FullName, user.Email, Role = user.Role.RoleName, user.CreatedAt });
        }

        /// <summary>
        /// Updates the name and email of the logged-in user.
        /// </summary>
        /// <remarks>Expects fullName and email. The email must not belong to another user.</remarks>
        /// <response code="200">The updated profile.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="409">The email is already in use by another user.</response>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateMe(UpdateProfileDto dto)
        {
            var user = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            if (user == null) return Unauthorized();

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != user.UserId))
                return Conflict("Email already in use.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            await _db.SaveChangesAsync();

            return Ok(new { user.UserId, user.FullName, user.Email, Role = user.Role.RoleName });
        }
    }
}
/// <summary>
/// Updates the name and email of the logged-in user.
/// </summary>
/// <remarks>Expects fullName and email. The email must not belong to another user.</remarks>
/// <response code="200">The updated profile.</response>
/// <response code="400">Validation failed.</response>
/// <response code="401">Not logged in.</response>
/// <response code="409">The email is already in use by another user.</response>
[HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateMe(UpdateProfileDto dto)
        {
            var user = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            if (user == null) return Unauthorized();

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != user.UserId))
                return Conflict("Email already in use.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            await _db.SaveChangesAsync();

            return Ok(new { user.UserId, user.FullName, user.Email, Role = user.Role.RoleName });
        }
    }
}