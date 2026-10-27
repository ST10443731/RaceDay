using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [Route("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;
        public OrganiserController(ApiClient api) { _api = api; }

        private bool IsOrganiser()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Organiser" || role == "Organizer";
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.Get("api/events");
            if (!response.IsSuccess)
            {
                ViewBag.Error = response.ErrorMessage();
                return View(new List<EventViewModel>());
            }

            var events = response.As<List<EventViewModel>>() ?? new List<EventViewModel>();
            return View(events);
        }

        [HttpGet("CreateEvent")]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            return View(new CreateEventViewModel());
        }

        [HttpPost("CreateEvent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View(model);

            var formData = new Dictionary<string, string>
            {
                { "Name", model.Name },
                { "Description", model.Description ?? "" },
                { "EventDate", model.Date.ToString("yyyy-MM-ddTHH:mm:ss") },
                { "Location", model.Location },
                { "EventType", model.EventType },
                { "RouteInfo", model.RouteInfo }
            };

            var response = await _api.PostForm("api/events", formData, model.BannerImage);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError("", response.ErrorMessage());
                return View(model);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction("Dashboard");
        }

        [HttpGet("EventDetails/{id}")]
        public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var eventResponse = await _api.Get($"api/events/{id}");
            if (!eventResponse.IsSuccess) return NotFound();
            var ev = eventResponse.As<EventViewModel>();

            var catResponse = await _api.Get($"api/events/{id}/categories");
            var categories = catResponse.IsSuccess ? catResponse.As<List<CategoryViewModel>>() ?? new List<CategoryViewModel>() : new List<CategoryViewModel>();

            ViewBag.Categories = categories;
            ViewBag.EventName = ev?.Name ?? "Event";
            
            return View(ev);
        }

        [HttpPost("AddCategory/{eventId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int eventId, CreateCategoryViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return RedirectToAction("EventDetails", new { id = eventId });

            var response = await _api.Post($"api/events/{eventId}/categories", new
            {
                name = model.Name,
                distanceKm = model.DistanceKm,
                maxParticipants = model.MaxParticipants
            });

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.ErrorMessage();
            }
            else
            {
                TempData["Success"] = "Category added successfully!";
            }

            return RedirectToAction("EventDetails", new { id = eventId });
        }

        [HttpGet("ViewEnrolments/{eventId}")]
        public async Task<IActionResult> ViewEnrolments(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var eventResponse = await _api.Get($"api/events/{eventId}");
            ViewBag.EventName = eventResponse.IsSuccess ? eventResponse.As<EventViewModel>()?.Name : "Event";

            var response = await _api.Get($"api/events/{eventId}/enrolments");
            if (!response.IsSuccess)
            {
                ViewBag.Error = response.ErrorMessage();
                return View(new List<OrganiserEnrolmentViewModel>());
            }

            var enrolments = response.As<List<OrganiserEnrolmentViewModel>>() ?? new List<OrganiserEnrolmentViewModel>();
            return View(enrolments);
        }

        // --- NEW: RESULTS ENTRY ---
        [HttpGet("ResultsEntry/{eventId}")]
        public async Task<IActionResult> ResultsEntry(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var eventResponse = await _api.Get($"api/events/{eventId}");
            ViewBag.EventName = eventResponse.IsSuccess ? eventResponse.As<EventViewModel>()?.Name : "Event";
            ViewBag.EventId = eventId;

            var response = await _api.Get($"api/events/{eventId}/enrolments");
            if (!response.IsSuccess)
            {
                ViewBag.Error = response.ErrorMessage();
                return View(new List<OrganiserEnrolmentViewModel>());
            }

            var enrolments = response.As<List<OrganiserEnrolmentViewModel>>() ?? new List<OrganiserEnrolmentViewModel>();
            return View(enrolments);
        }

        [HttpPost("CaptureResult/{enrolmentId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaptureResult(int enrolmentId, string finishTime, int position, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var response = await _api.Post($"api/enrolments/{enrolmentId}/result", new
            {
                finishTime = finishTime,
                position = position
            });

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.ErrorMessage();
            }
            else
            {
                TempData["Success"] = "Result captured successfully!";
            }

            return RedirectToAction("ResultsEntry", new { eventId = eventId });
        }
    }
}