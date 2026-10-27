using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public string? Role { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (Role != null && role != Role)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}