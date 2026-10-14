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
    [Tags("Results")]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ResultsController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Captures the finish time and position for a participant's enrolment.
        /// </summary>
        /// <remarks>
        /// Organizer only, and only for their own events. Expects finishTime (for example "00:52:30")
        /// and position (1 or higher). Each enrolment can have only one result.
        /// </remarks>
        /// <response code="201">Result captured. Returns the result id, finish time and position.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organizer, or not the owner of this event.</response>
        /// <response code="404">The enrolment does not exist.</response>
        /// <response code="409">A result has already been captured for this enrolment.</response>
        [HttpPost("api/enrolments/{enrolmentId}/result")]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Capture(int enrolmentId, ResultDto dto)
        {
            var enrolment = await _db.Enrolments
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null) return NotFound("Enrolment does not exist.");
            if (enrolment.Category.Event.OrganizerId != CurrentUserId) return Forbid();

            if (await _db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId))
                return Conflict("A result has already been captured for this enrolment.");

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { result.ResultId, result.FinishTime, result.Position });
        }

        /// <summary>
        /// Returns the logged-in Participant's own result history.
        /// </summary>
        /// <remarks>Participant only. No body needed.</remarks>
        /// <response code="200">An array of the participant's results across all events.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">The user is not a Participant.</response>
        [HttpGet("api/users/me/results")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMine()
        {
            var results = await _db.Results
                .Where(r => r.Enrolment.ParticipantId == CurrentUserId)
                .Select(r => new
                {
                    r.ResultId,
                    Event = r.Enrolment.Category.Event.Name,
                    r.Enrolment.Category.Event.EventDate,
                    Category = r.Enrolment.Category.Name,
                    r.FinishTime,
                    r.Position,
                    r.RecordedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Returns all results for an event, ordered by position.
        /// </summary>
        /// <remarks>No body needed. Available to both roles.</remarks>
        /// <response code="200">An array of results for the event.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="404">The event does not exist.</response>
        [HttpGet("api/events/{eventId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound("Event does not exist.");

            var results = await _db.Results
                .Where(r => r.Enrolment.Category.EventId == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new
                {
                    r.Position,
                    Participant = r.Enrolment.Participant.FullName,
                    Category = r.Enrolment.Category.Name,
                    r.FinishTime
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
