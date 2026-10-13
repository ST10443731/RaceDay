using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Tags("Categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Lists all categories for an event.
        /// </summary>
        /// <remarks>No body needed. Available to both roles.</remarks>
        /// <response code="200">An array of categories for the event.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="404">The event does not exist.</response>
        [HttpGet("api/events/{eventId}/categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound("Event does not exist.");

            var categories = await _db.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new { c.CategoryId, c.Name, c.DistanceKm, c.MaxParticipants })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Adds a category to an event owned by the logged-in Organizer.
        /// </summary>
        /// <remarks>Organizer only. Expects name, distanceKm and maxParticipants.</remarks>
        /// <response code="201">Category created. Returns the new category id.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organizer, or not the owner of this event.</response>
        /// <response code="404">The event does not exist.</response>
        [HttpPost("api/events/{eventId}/categories")]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(int eventId, CategoryDto dto)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event does not exist.");
            if (ev.OrganizerId != CurrentUserId) return Forbid();

            var category = new Category
            {
                EventId = eventId,
                Name = dto.Name,
                DistanceKm = dto.DistanceKm,
                MaxParticipants = dto.MaxParticipants
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { category.CategoryId });
        }

        /// <summary>
        /// Updates a category of an event owned by the logged-in Organizer.
        /// </summary>
        /// <remarks>Organizer only. Expects name, distanceKm and maxParticipants.</remarks>
        /// <response code="200">The updated category.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organizer, or not the owner of this event.</response>
        /// <response code="404">The category does not exist.</response>
        [HttpPut("api/categories/{id}")]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, CategoryDto dto)
        {
            var category = await _db.Categories.Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            if (category.Event.OrganizerId != CurrentUserId) return Forbid();

            category.Name = dto.Name;
            category.DistanceKm = dto.DistanceKm;
            category.MaxParticipants = dto.MaxParticipants;
            await _db.SaveChangesAsync();

            return Ok(new { category.CategoryId, category.Name, category.DistanceKm, category.MaxParticipants });
        }

        /// <summary>
        /// Removes a category from an event owned by the logged-in Organizer.
        /// </summary>
        /// <remarks>Organizer only. The category's enrolments and results are deleted too.</remarks>
        /// <response code="204">Category deleted.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organizer, or not the owner of this event.</response>
        /// <response code="404">The category does not exist.</response>
        [HttpDelete("api/categories/{id}")]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            if (category.Event.OrganizerId != CurrentUserId) return Forbid();

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
