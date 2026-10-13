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
    [Authorize]   // both roles get in; stricter rules go on individual methods
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EventsController(AppDbContext db) { _db = db; }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _db.Events
                .Where(e => e.EventDate >= DateTime.Now)   // "upcoming" events only
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

        [HttpGet("{id}")]
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

        [HttpPost]
        [Authorize(Roles = "Organizer")]   // a Participant gets 403 here
        public async Task<IActionResult> Create(EventDto dto)
        {
            var ev = new Event
            {
                OrganizerId = CurrentUserId,   // from the cookie, not from the request
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Update(int id, EventDto dto)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();                     // 404: doesn't exist
            if (ev.OrganizerId != CurrentUserId) return Forbid();  // 403: exists, but not yours

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.EventType = dto.EventType;
            ev.RouteInfo = dto.RouteInfo;
            await _db.SaveChangesAsync();

            return Ok(new { ev.EventId, ev.Name, ev.EventDate, ev.Location, ev.EventType });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganizerId != CurrentUserId) return Forbid();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            return NoContent();   // 204, as in your plan
        }
    }
}
