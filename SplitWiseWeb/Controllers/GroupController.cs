using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;

namespace SplitWiseWeb.Controllers;

public class GroupController : Controller
{
    public GroupController()
    {
    }

    [Breadcrumb("Groups")]
    [Route("groups")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

}
