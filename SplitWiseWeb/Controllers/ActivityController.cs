using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
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
        List<Activity> activities = await _activityService.UserActivityList();
        ViewData["ActiveLink"] = "Activities";
        return View(activities);
    }

    // GET GroupActivities
    public async Task<IActionResult> GroupActivities(int groupId)
    {
        List<Activity> activities = await _activityService.GroupActivityList(groupId);
        return PartialView("ActivityList", activities);
    }
}
