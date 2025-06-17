using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;

namespace SplitWiseWeb.Controllers;

// [Authorize]
[Breadcrumb("Friends")]
public class FriendController : Controller
{
    public FriendController()
    {
    }

    // GET Index
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Friends";
        return View();
    }

}
