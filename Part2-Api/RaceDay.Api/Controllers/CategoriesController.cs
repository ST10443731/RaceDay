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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Both roles can view the categories of an event
        [HttpGet("api/events/{eventId}/categories")]
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

        [HttpPost("api/events/{eventId}/categories")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Create(int eventId, CategoryDto dto)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event does not exist.");
            if (ev.OrganizerId != CurrentUserId) return Forbid();   // not your event

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

        [HttpPut("api/categories/{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Update(int id, CategoryDto dto)
        {
            // Include(c => c.Event) loads the parent event, so we can see who owns it
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

        [HttpDelete("api/categories/{id}")]
        [Authorize(Roles = "Organizer")]
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
