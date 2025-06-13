using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    #region Login
    // GET Login
    [AllowAnonymous]
    public IActionResult Login()
    {
        string? jwtToken = Request.Cookies["JwtToken"];
        string? rememberMeToken = Request.Cookies["RememberMeToken"];
        if (string.IsNullOrEmpty(rememberMeToken) && string.IsNullOrEmpty(jwtToken))
        {
            return View();
        }

        // Redirect to dashboard
        return RedirectToAction("Index", "Dashboard");
    }

    // POST Login
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginVM loginVM)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        ResponseVM response = await _authService.ValidateUser(loginVM.Email, loginVM.Password);
        if (!response.Success)
        {
            TempData["errorMessage"] = response.Message;
            return View(loginVM);
        }
        else
        {
            TempData["successMessage"] = response.Message;
        }

        // Set Cookies 
        CookieOptions options = new CookieOptions
        {
            Expires = loginVM.IsRememberMe ? DateTime.Now.AddHours(24) : DateTime.Now.AddHours(1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("JwtToken", response.Token, options);
        Response.Cookies.Append("UserName", response.Name, options);
        
        if (loginVM.IsRememberMe)
        {
            Response.Cookies.Append("RememberMeToken", response.Token, options);
        }

        // Redirect to dashboard
        return RedirectToAction("Index", "Dashboard");
    }
    #endregion

    #region Register
    // GET Register
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    // GET Register
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterUserVM registerUserVM)
    {
        if (!ModelState.IsValid)
        {
            return View(registerUserVM);
        }

        // Add User
        ResponseVM response = await _authService.RegisterUser(registerUserVM);
        if (response.Success)
        {
            TempData["successMessage"] = response.Message;
        }
        else
        {
            TempData["errorMessage"] = response.Message;
        }

        return RedirectToAction("Login");
    }
    #endregion

    #region User Verification
    // GET UserVerification
    [AllowAnonymous]
    public async Task<IActionResult> UserVerification(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["errorMessage"] = "Invalid Token";
            return RedirectToAction("Login");
        }

        ResponseVM response = await _authService.UserVerification(token);
        if (response.Success)
        {
            TempData["successMessage"] = response.Message;
        }
        else
        {
            TempData["errorMessage"] = response.Message;
        }
        return RedirectToAction("Login");
    }
    #endregion

    #region Forgot Password
    // GET ForgotPassword
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }
    #endregion

    #region Logout
    // GET Logout
    public IActionResult Logout()
    {
        // Clear Session And Cookies
        HttpContext.Session.Clear();
        Response.Cookies.Delete("JwtToken");
        Response.Cookies.Delete("RememberMeToken");
        Response.Cookies.Delete("UserName");

        return RedirectToAction("Login");
    }
    #endregion

    #region Error
    [AllowAnonymous]
    [Route("/Auth/Error/{code}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int code)
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [AllowAnonymous]
    public IActionResult HandleExceptionWithToaster(string message)
    {
        TempData["ToastError"] = message;

        string referer = Request.Headers["Referer"].ToString();

        if (string.IsNullOrEmpty(referer))
        {
            referer = Url.Action("Login", "Auth") ?? "/";
        }

        return Redirect(referer);
    }
    #endregion
}
