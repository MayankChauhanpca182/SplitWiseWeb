using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    #region Login
    // GET Login
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
    public async Task<IActionResult> Login(LoginVM loginVM)
    {
        if (!ModelState.IsValid)
        {
            return View(loginVM);
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

        // Fetch user
        User? user = await _userService.GetByEmailAddress(loginVM.Email);

        Response.Cookies.Append("JwtToken", response.StringValue, options);
        Response.Cookies.Append("UserName", $"{user.FirstName} {user.LastName}", options);

        if (!string.IsNullOrEmpty(user.ProfileImagePath))
        {
            Response.Cookies.Append("ProfileImageUrl", user.ProfileImagePath, options);
        }
        if (loginVM.IsRememberMe)
        {
            Response.Cookies.Append("RememberMeToken", response.StringValue, options);
        }

        // Redirect to dashboard
        return RedirectToAction("Index", "Dashboard");
    }
    #endregion

    #region Register
    // GET Register
    public IActionResult Register()
    {
        return View();
    }

    // POST Register
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
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST ForgotPassword
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(LoginVM loginVM)
    {
        ModelState.Remove("Password");
        if (!ModelState.IsValid)
        {
            return View(loginVM);
        }

        // Send email with reset password link
        ResponseVM response = await _authService.ForgotPassword(loginVM.Email);
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

    #region Reset Password
    // GET ResetPassword
    public async Task<IActionResult> ResetPassword(string? token = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["errorMessage"] = NotificationMessages.Invalid.Replace("{0}", "Token");
            return RedirectToAction("Login");
        }

        // Validate token using expiry (ValidatePasswordResetToken)
        ResponseVM response = await _authService.ValidatePasswordResetToken(token);
        return View(new RegisterUserVM() { Email = response.StringValue });
    }

    // POST ResetPassword
    [HttpPost]
    public async Task<IActionResult> ResetPassword(RegisterUserVM registerUserVM)
    {
        ModelState.Remove("FirstName");
        ModelState.Remove("LastName");
        if (!ModelState.IsValid)
        {
            return View(registerUserVM);
        }

        // Reset passsword
        // ResponseVM response = await _authService.ResetPassword(registerUserVM);

        // if (response.Success)
        // {
        //     TempData["successMessage"] = response.Message;
        // }
        // else
        // {
        //     TempData["errorMessage"] = response.Message;
        // }

        return RedirectToAction("Login");
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
    [Route("/Auth/Error/{code}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int code)
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

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
