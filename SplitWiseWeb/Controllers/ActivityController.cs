using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

public class ActivityController : Controller
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    // GET UserActivities
    [Breadcrumb("User Activities")]
    [Route("user-activities")]
    public async Task<IActionResult> Index()
    {
        ActivityFilterVM activityFilter = await _activityService.GetActivityFilter();
        ViewData["ActiveLink"] = "Activities";
        return View(activityFilter);
    }

    // POST ActivityList
    public async Task<IActionResult> ActivityList(FilterVM filter, int? groupId = null)
    {
        List<Activity> activities = await  _activityService.ActivityList(filter, groupId);
        return PartialView("ActivityList", activities);
    }
}
