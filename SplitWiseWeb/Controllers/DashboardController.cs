using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
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
    [Route("home")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Dashboard";
        return View();
    }

    [Route("dashboard")]
    [Breadcrumb("Dashboard")]
    public IActionResult Dashboard()
    {
        return PartialView("Dashboard");
    }
    #endregion


}
