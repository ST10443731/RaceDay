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
    [Tags("Event Enrolments")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EnrolmentsController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Enrols the logged-in Participant in a category.
        /// </summary>
        /// <remarks>Participant only. No body needed: the category is in the URL and the participant comes from the session.</remarks>
        /// <response code="201">Enrolled. Returns the enrolment id and status.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">The user is not a Participant.</response>
        /// <response code="404">The category does not exist.</response>
        /// <response code="409">Already enrolled in this category, or the category is full.</response>
        [HttpPost("api/categories/{categoryId}/enrolments")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            var category = await _db.Categories.FindAsync(categoryId);
            if (category == null) return NotFound("Category does not exist.");

            if (await _db.Enrolments.AnyAsync(e =>
                    e.CategoryId == categoryId && e.ParticipantId == CurrentUserId))
                return Conflict("You are already enrolled in this category.");

            var enrolled = await _db.Enrolments.CountAsync(e => e.CategoryId == categoryId);
            if (enrolled >= category.MaxParticipants)
                return Conflict("This category is full.");

            var enrolment = new Enrolment
            {
                ParticipantId = CurrentUserId,
                CategoryId = categoryId
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { enrolment.EnrolmentId, enrolment.Status });
        }

        /// <summary>
        /// Lists all enrolments of the logged-in Participant.
        /// </summary>
        /// <remarks>Participant only. No body needed.</remarks>
        /// <response code="200">An array of the participant's enrolments, with category and event details.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">The user is not a Participant.</response>
        [HttpGet("api/users/me/enrolments")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Lists all enrolments for an event owned by the logged-in Organizer.
        /// </summary>
        /// <remarks>Organizer only, and only for their own events. No body needed.</remarks>
        /// <response code="200">An array of enrolments with participant and category details.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organizer, or not the owner of this event.</response>
        /// <response code="404">The event does not exist.</response>
        [HttpGet("api/events/{eventId}/enrolments")]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Withdraws the logged-in Participant from an event category.
        /// </summary>
        /// <remarks>Participant only, and only for their own enrolment. No body needed.</remarks>
        /// <response code="204">Enrolment removed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not a Participant, or not the owner of this enrolment.</response>
        /// <response code="404">The enrolment does not exist.</response>
        [HttpDelete("api/enrolments/{id}")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Withdraw(int id)
        {
            var enrolment = await _db.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();
            if (enrolment.ParticipantId != CurrentUserId) return Forbid();

            _db.Enrolments.Remove(enrolment);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}