using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GLMS_Web.Controllers;
// Redirects users to the login page if no JWT token exists in Session
public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var token = HttpContext.Session.GetString("JwtToken");

        if (string.IsNullOrEmpty(token))
        {
            context.Result = RedirectToAction("Login", "Account");
            return;
        }

        base.OnActionExecuting(context);
    }
}