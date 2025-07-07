using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

    // GET GroupActivities
    public async Task<IActionResult> GroupActivities(int groupId)
    {
        List<ActivityVM> activities = await _activityService.GroupActivityList(groupId);
        return Json(activities);
    }

}
