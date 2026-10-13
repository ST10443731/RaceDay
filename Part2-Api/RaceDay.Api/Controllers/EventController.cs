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
    [Route("api/events")]
    [Authorize]
    [Tags("Events")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EventsController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Lists all upcoming events.
        /// </summary>
        /// <remarks>No body needed. Available to both roles. Only events dated today or later are returned.</remarks>
        /// <response code="200">An array of upcoming events, soonest first.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var events = await _db.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.EventType,
                    e.RouteInfo,
                    Organizer = e.Organizer.FullName
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Returns the full details of one event, including its categories.
        /// </summary>
        /// <response code="200">The event and its categories.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="404">The event does not exist.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _db.Events
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.EventType,
                    e.RouteInfo,
                    Organizer = e.Organizer.FullName,
                    Categories = e.Categories.Select(c => new
                    { c.CategoryId, c.Name, c.DistanceKm, c.MaxParticipants })
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound();
            return Ok(ev);
        }

        /// <summary>
        /// Creates a new event owned by the logged-in Organizer.
        /// </summary>
        /// <remarks>
        /// Organizer only. Expects name, description, eventDate, location,
        /// eventType (run, walk or cycle) and routeInfo.
        /// </remarks>
        /// <response code="201">Event created. Returns the new event id.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">The user is not an Organizer.</response>
        [HttpPost]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(EventDto dto)
        {
            var ev = new Event
            {
                OrganizerId = CurrentUserId,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                EventType = dto.EventType,
                RouteInfo = dto.RouteInfo
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new { ev.EventId });
        }

        /// <summary>
        /// Updates an event owned by the logged-in Organizer.
        /// </summary>
        /// <remarks>Organizer only, and only for their own events. Expects the same body as creating an event.</remarks>
        /// <response code="200">The updated event.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organizer, or not the owner of this event.</response>
        /// <response code="404">The event does not exist.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, EventDto dto)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganizerId != CurrentUserId) return Forbid();

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.EventType = dto.EventType;
            ev.RouteInfo = dto.RouteInfo;
            await _db.SaveChangesAsync();

            return Ok(new { ev.EventId, ev.Name, ev.EventDate, ev.Location, ev.EventType });
        }

        /// <summary>
        /// Deletes an event owned by the logged-in Organizer.
        /// </summary>
        /// <remarks>Organizer only, and only for their own events. Its categories, enrolments and results are deleted too.</remarks>
        /// <response code="204">Event deleted.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organizer, or not the owner of this event.</response>
        /// <response code="404">The event does not exist.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organizer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganizerId != CurrentUserId) return Forbid();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
