using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
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
    [HttpPost]
    public async Task<IActionResult> ActivityList(FilterVM filter, int? groupId = null, int? friendUserId = null)
    {
        List<Activity> activities = await _activityService.ActivityList(filter, groupId, friendUserId);
        return PartialView("ActivityList", activities);
    }

    // POST ExportActivity
    [HttpPost]
    public async Task<IActionResult> ExportActivity(FilterVM filter, int? groupId = null, int? friendUserId = null)
    {
        byte[] fileData = await _activityService.ExportActivity(filter, groupId: groupId, friendUserId: friendUserId);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "activities") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Activities.xlsx");
    }
}
