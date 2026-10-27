using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiClient _api;
        public ParticipantController(ApiClient api) { _api = api; }

        private bool IsParticipant()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Participant";
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.Post($"api/categories/{categoryId}/enrolments", new { });

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.ErrorMessage();
            }
            else
            {
                TempData["Success"] = "You have successfully enrolled!";
            }

            return RedirectToAction("EventDetails", "Home", new { id = eventId });
        }

        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.Get("api/users/me/enrolments"); 
            
            if (!response.IsSuccess)
            {
                ViewBag.Error = response.ErrorMessage();
                return View(new List<EnrolmentViewModel>());
            }

            var enrolments = response.As<List<EnrolmentViewModel>>() ?? new List<EnrolmentViewModel>();
            return View(enrolments);
        }

        // --- NEW: MY RESULTS ---
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var response = await _api.Get("api/users/me/results"); 
            
            if (!response.IsSuccess)
            {
                ViewBag.Error = response.ErrorMessage();
                return View(new List<ResultViewModel>());
            }

            var results = response.As<List<ResultViewModel>>() ?? new List<ResultViewModel>();
            return View(results);
        }
    }
}