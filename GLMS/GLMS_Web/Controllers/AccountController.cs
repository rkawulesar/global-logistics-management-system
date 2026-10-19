using GLMS_Web.ApiServices;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_Web.Controllers;
//Code Attribution for session and state management
//Link: https://www.c-sharpcorner.com/blogs/understanding-state-management-in-net-core
public class AccountController : Controller
{
    private readonly AuthApiService _authApiService;

    public AccountController(AuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Sends login request to the API
        var response = await _authApiService.LoginAsync(request);

        // Invalid login credentials
        if (response == null)
        {
            ViewBag.Error = "Invalid username or password.";

            return View(request);
        }

        // Stores JWT token in Session
        HttpContext.Session.SetString("JwtToken", response.Token);

        // Stores user role in Session
        HttpContext.Session.SetString("UserRole", response.Role);

        return RedirectToAction("Index", "Home");
    }

    // GET: Account/Logout
    public IActionResult Logout()
    {
        // Clears Session on logout
        HttpContext.Session.Clear();

        return RedirectToAction(nameof(Login));
    }
}