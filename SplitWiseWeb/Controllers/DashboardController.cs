using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
[DefaultBreadcrumb("Home")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserService _userService;

    public DashboardController(IDashboardService dashboardService, IUserService userService)
    {
        _dashboardService = dashboardService;
        _userService = userService;
    }

    #region Dashboard
    // GET Index
    [Route("dashboard")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Dashboard";
        return View();
    }
    #endregion

    #region Change Password
    // GET ChangePassword
    [Breadcrumb("Change Password", FromAction = "Index")]
    [Route("changePassword")]
    public IActionResult ChangePassword()
    {
        ViewData["ActiveLink"] = "Change Password";
        return View();
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

        ResponseVM response = await _dashboardService.ChangePassword(passwordReset);
        if (!response.Success)
        {
            TempData["errorMessage"] = response.Message;
            return View(passwordReset);
        }

        TempData["successMessage"] = response.Message;
        return RedirectToAction("Logout", "Auth");
    }
    #endregion

    #region Profile
    // GET Profile
    [Breadcrumb("Profile", FromAction = "Index")]
    [Route("profile")]
    public async Task<IActionResult> Profile()
    {
        ViewData["ActiveLink"] = "Profile";
        return View(await _dashboardService.GetProfile());
    }

    // POST Profile
    [HttpPost]
    [Route("profile")]
    public async Task<IActionResult> Profile(ProfileVM profile)
    {
        if (!ModelState.IsValid)
        {
            return View(await _dashboardService.GetProfile());
        }

        ResponseVM response = await _dashboardService.UpdateProfile(profile);
        if (!response.Success)
        {
            TempData["errorMessage"] = response.Message;
            return View(await _dashboardService.GetProfile());
        }
        else
        {
            int userId = await _userService.LoggedInUserId();
            User user = await _userService.GetById(userId);

            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(5),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("UserName", $"{user.FirstName} {user.LastName}", options);
            Response.Cookies.Append("ProfileImagePath", user.ProfileImagePath, options);

            TempData["successMessage"] = response.Message;
        }

        return RedirectToAction("Profile");
    }
    #endregion
}
