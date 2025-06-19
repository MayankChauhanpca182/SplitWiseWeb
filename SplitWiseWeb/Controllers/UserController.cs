using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IPasswordResetService _passwordResetService;


    public UserController(IUserService userService, IPasswordResetService passwordResetService)
    {
        _userService = userService;
        _passwordResetService = passwordResetService;
    }

    #region Register
    // GET Register
    [Route("register")]
    public IActionResult Register()
    {
        return View();
    }

    // POST Register
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterUserVM registerUserVM)
    {
        if (!ModelState.IsValid)
        {
            return View(registerUserVM);
        }

        // Add User
        ResponseVM response = await _userService.RegisterUser(registerUserVM);
        if (response.Success)
        {
            TempData["successMessage"] = response.Message;
        }
        else
        {
            TempData["errorMessage"] = response.Message;
        }

        return RedirectToAction("Login", "Auth");
    }
    #endregion

    #region Reset Password
    // GET ResetPassword
    [Route("resetPassword")]
    public async Task<IActionResult> ResetPassword(string? token = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["errorMessage"] = NotificationMessages.Invalid.Replace("{0}", "Token");
            return RedirectToAction("Login", "Auth");
        }

        // Validate token using expiry
        ResponseVM response = await _passwordResetService.Validate(token);
        if (!response.Success)
        {
            TempData["errorMessage"] = response.Message;
            return RedirectToAction("Login", "Auth");
        }
        return View(new PasswordResetVM() { ResetToken = token });
    }

    // POST ResetPassword
    [HttpPost]
    [Route("resetPassword")]
    public async Task<IActionResult> ResetPassword(PasswordResetVM passwordReset)
    {
        ModelState.Remove("Password");
        if (!ModelState.IsValid)
        {
            return View(passwordReset);
        }

        // Reset passsword
        ResponseVM response = await _passwordResetService.ResetPassword(passwordReset);

        if (response.Success)
        {
            TempData["successMessage"] = response.Message;
        }
        else
        {
            TempData["errorMessage"] = response.Message;
        }
        return RedirectToAction("Login", "Auth");
    }
    #endregion

    #region Change Password
    // GET ChangePassword
    [Breadcrumb("Change Password", FromAction = "Index", FromController = typeof(DashboardController))]
    [Route("changePassword")]
    public IActionResult ChangePassword()
    {
        return PartialView("ChangePassword");
    }

    // POST ChangePassword
    [HttpPost]
    [Route("changePassword")]
    public async Task<IActionResult> ChangePassword(PasswordResetVM passwordReset)
    {
        if (!ModelState.IsValid)
        {
            return View(passwordReset);
        }

        ResponseVM response = await _passwordResetService.ChangePassword(passwordReset);
        return Json(response);
        // if (!response.Success)
        // {
        //     TempData["errorMessage"] = response.Message;
        //     return View(passwordReset);
        // }

        // TempData["successMessage"] = response.Message;
        // return RedirectToAction("Logout", "Auth");
    }
    #endregion

    #region Profile
    // GET Profile
    [Breadcrumb("Profile", FromAction = "Index", FromController = typeof(DashboardController))]
    [Route("profile")]
    public async Task<IActionResult> Profile()
    {
        return PartialView("Profile", await _userService.GetProfile());
    }

    // POST Profile
    [HttpPost]
    [Route("profile")]
    public async Task<IActionResult> Update(UserVM newUser)
    {
        if (!ModelState.IsValid)
        {
            // return View("Profile", await _userService.GetProfile());
            return PartialView("Profile", await _userService.GetProfile());
        }

        ResponseVM response = await _userService.Update(newUser);
        User user = await _userService.LoggedInUser();
        CookieOptions options = new CookieOptions
        {
            Expires = DateTime.Now.AddHours(24),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };
        Response.Cookies.Append("UserName", $"{user.FirstName} {user.LastName}", options);
        Response.Cookies.Append("ProfileImagePath", user.ProfileImagePath, options);
        return Json(response);

        // if (!response.Success)
        // {
        //     TempData["errorMessage"] = response.Message;
        //     return View("Profile", await _userService.GetProfile());
        // }
        // else
        // {
        //     User user = await _userService.LoggedInUser();

        //     CookieOptions options = new CookieOptions
        //     {
        //         Expires = DateTime.Now.AddHours(24),
        //         HttpOnly = true,
        //         Secure = true,
        //         SameSite = SameSiteMode.Strict
        //     };
        //     Response.Cookies.Append("UserName", $"{user.FirstName} {user.LastName}", options);
        //     Response.Cookies.Append("ProfileImagePath", user.ProfileImagePath, options);

        //     TempData["successMessage"] = response.Message;
        // }

        // return RedirectToAction("Profile");
    }
    #endregion

}
