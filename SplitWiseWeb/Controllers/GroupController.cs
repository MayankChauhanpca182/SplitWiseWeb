using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

public class GroupController : Controller
{
    private readonly IGroupService _groupService;
    private readonly ICommonService _commonService;

    public GroupController(IGroupService groupService, ICommonService commonService)
    {
        _groupService = groupService;
        _commonService = commonService;
    }

    [Breadcrumb("Groups")]
    [Route("groups")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    // GET AddGroup
    // [Breadcrumb("Create Group", FromAction = "Index")]
    public async Task<IActionResult> AddGroup(int groupId = 0)
    {
        GroupVM group = new GroupVM();
        if (groupId > 0)
        {
            group = await _groupService.GetGroup(groupId);
        }
        group.Currencies = await _commonService.CurrencyList();
        return PartialView("AddGroupModal", group);
    }

    // POST SaveGroup
    public async Task<IActionResult> SaveGroup(GroupVM newGroupVM)
    {
        if (!ModelState.IsValid)
        {
            newGroupVM.Currencies = await _commonService.CurrencyList();
            return PartialView("AddGroup", newGroupVM);
        }

        ResponseVM response = await _groupService.SaveGroup(newGroupVM);
        return Json(response);
    }

    // POST GroupList
    public async Task<IActionResult> GroupList(FilterVM filter)
    {
        PaginatedListVM<GroupVM> paginatedList = await _groupService.GroupList(filter);
        return PartialView("GroupListPartialView", paginatedList);
    }

    // POST DeleteGroup
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        ResponseVM response = await _groupService.DeleteGroup(groupId);
        return Json(response);
    }
}
