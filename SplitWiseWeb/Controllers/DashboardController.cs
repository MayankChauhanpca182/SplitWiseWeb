using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET Index
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Dashboard";
        return View();
    }

    // GET ChangePassword
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST ChangePassword
    [HttpPost]
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

}
