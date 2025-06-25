using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
[DefaultBreadcrumb("Home")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    #region Dashboard
    // GET Index
    [Route("dashboard")]
    public async Task<IActionResult> Index()
    {
        DashboardVM dashboard = await _dashboardService.GetDashboard();
        ViewData["ActiveLink"] = "Dashboard";
        return View(dashboard);
    }

    #endregion

}
