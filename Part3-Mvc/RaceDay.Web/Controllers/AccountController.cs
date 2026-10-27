using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;
        public AccountController(ApiClient api) { _api = api; }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Role") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var login = await _api.Post("api/auth/login", new { email = model.Email, password = model.Password });
            if (!login.IsSuccess)
            {
                ModelState.AddModelError("", login.ErrorMessage());
                return View(model);
            }

            var cookie = login.SetCookie?.Split(';')[0];
            if (string.IsNullOrEmpty(cookie))
            {
                ModelState.AddModelError("", "We could not start your session. Please try again.");
                return View(model);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.SetString("ApiCookie", cookie);

            var result = login.As<LoginResult>();
            HttpContext.Session.SetString("Role", result!.Role);
            HttpContext.Session.SetString("UserId", result.UserId.ToString());

            var me = await _api.Get("api/users/me");
            HttpContext.Session.SetString("FullName", me.As<ProfileResult>()?.FullName ?? model.Email);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Role") != null)
                return RedirectToAction("Index", "Home");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _api.Post("api/auth/register", new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (!response.IsSuccess)
            {
                ModelState.AddModelError("", response.ErrorMessage());
                return View(model);
            }

            TempData["Success"] = "Your account was created. Please log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}