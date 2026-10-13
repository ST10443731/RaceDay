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
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ResultsController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("api/enrolments/{enrolmentId}/result")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Capture(int enrolmentId, ResultDto dto)
        {
            // Load the enrolment AND its category AND that category's event, so we can see who owns it
            var enrolment = await _db.Enrolments
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null) return NotFound("Enrolment does not exist.");
            if (enrolment.Category.Event.OrganizerId != CurrentUserId) return Forbid();   // not your event

            // One result per enrolment
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

        [HttpGet("api/users/me/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMine()
        {
            var results = await _db.Results
                .Where(r => r.Enrolment.ParticipantId == CurrentUserId)   // only MY results
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

        [HttpGet("api/events/{eventId}/results")]
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
