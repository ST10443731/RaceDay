using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EnrolmentsController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("api/categories/{categoryId}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            var category = await _db.Categories.FindAsync(categoryId);
            if (category == null) return NotFound("Category does not exist.");

            // Rule 1: one enrolment per participant per category
            if (await _db.Enrolments.AnyAsync(e =>
                    e.CategoryId == categoryId && e.ParticipantId == CurrentUserId))
                return Conflict("You are already enrolled in this category.");

            // Rule 2: the category must have space left
            var enrolled = await _db.Enrolments.CountAsync(e => e.CategoryId == categoryId);
            if (enrolled >= category.MaxParticipants)
                return Conflict("This category is full.");

            var enrolment = new Enrolment
            {
                ParticipantId = CurrentUserId,   // from the cookie
                CategoryId = categoryId
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { enrolment.EnrolmentId, enrolment.Status });
        }

        [HttpGet("api/users/me/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMine()
        {
            var enrolments = await _db.Enrolments
                .Where(e => e.ParticipantId == CurrentUserId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EnrolmentDate,
                    e.Status,
                    Category = e.Category.Name,
                    e.Category.DistanceKm,
                    Event = e.Category.Event.Name,
                    e.Category.Event.EventDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpGet("api/events/{eventId}/enrolments")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event does not exist.");
            if (ev.OrganizerId != CurrentUserId) return Forbid();

            var enrolments = await _db.Enrolments
                .Where(e => e.Category.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EnrolmentDate,
                    e.Status,
                    Category = e.Category.Name,
                    Participant = e.Participant.FullName,
                    ParticipantEmail = e.Participant.Email
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpDelete("api/enrolments/{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Withdraw(int id)
        {
            var enrolment = await _db.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();
            if (enrolment.ParticipantId != CurrentUserId) return Forbid();   // not your enrolment

            _db.Enrolments.Remove(enrolment);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}