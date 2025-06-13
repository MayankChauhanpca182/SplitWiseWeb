using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public DashboardController()
    {
    }

    // GET Index
    public IActionResult Index()
    {
        return View();
    }

}
