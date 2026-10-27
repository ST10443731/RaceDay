using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiClient _api;
        public HomeController(ApiClient api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            // If not logged in, redirect to Login
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch all upcoming events from the API
            var response = await _api.Get("api/events");
            
            if (!response.IsSuccess)
            {
                ViewBag.Error = response.ErrorMessage();
                return View(new List<EventViewModel>());
            }

            var events = response.As<List<EventViewModel>>() ?? new List<EventViewModel>();
            return View(events);
        }

        // --- NEW: Participant Event Details ---
        public async Task<IActionResult> EventDetails(int id)
        {
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch the event details
            var eventResponse = await _api.Get($"api/events/{id}");
            if (!eventResponse.IsSuccess) return NotFound();
            var ev = eventResponse.As<EventViewModel>();

            // Fetch the categories for this event
            var catResponse = await _api.Get($"api/events/{id}/categories");
            var categories = catResponse.IsSuccess ? catResponse.As<List<CategoryViewModel>>() ?? new List<CategoryViewModel>() : new List<CategoryViewModel>();

            ViewBag.Categories = categories;
            
            return View(ev);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}