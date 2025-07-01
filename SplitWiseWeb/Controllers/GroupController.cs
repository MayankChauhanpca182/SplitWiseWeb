using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class GroupController : Controller
{
    private readonly IGroupService _groupService;
    private readonly ICommonService _commonService;
    private readonly IUserService _userService;

    public GroupController(IGroupService groupService, ICommonService commonService, IUserService userService)
    {
        _groupService = groupService;
        _commonService = commonService;
        _userService = userService;
    }

    [Breadcrumb("Groups")]
    [Route("groups")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Groups";
        return View("Index");
    }

    // GET AddGroupModal
    public async Task<IActionResult> AddGroupModal(int groupId = 0)
    {
        GroupVM group = new GroupVM();
        if (groupId > 0)
        {
            group = await _groupService.GetGroup(groupId);
        }
        group.Currencies = await _commonService.CurrencyList();
        return PartialView("AddGroupModalPartialView", group);
    }

    // POST SaveGroup
    [HttpPost]
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
    [HttpPost]
    public async Task<IActionResult> GroupList(FilterVM filter)
    {
        PaginatedListVM<GroupVM> paginatedList = await _groupService.GroupList(filter);
        return PartialView("GroupListPartialView", paginatedList);
    }

    // POST DeleteGroup
    [HttpPost]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        ResponseVM response = await _groupService.DeleteGroup(groupId);
        return Json(response);
    }

    // GET GroupDetails
    [Route("group-details/{groupId}")]
    [Breadcrumb("Group Details", FromAction = "Index")]
    public async Task<IActionResult> GroupDetails(int groupId)
    {
        GroupVM group = await _groupService.GetGroup(groupId);
        group.Members = await _groupService.GetMembers(groupId);
        ViewData["ActiveLink"] = "Groups";
        return View("GroupDetails", group);
    }

    // GET GroupMembers
    public async Task<IActionResult> GroupMembers(int groupId)
    {
        List<GroupMemberVM> members = await _groupService.GetMembers(groupId);
        return PartialView("GroupMembersPartialView", members);
    }

    // GET GroupMembersJson
    public async Task<IActionResult> GroupMembersJson(int groupId)
    {
        List<GroupMemberVM> members = await _groupService.GetMembers(groupId);
        return Json(members);
    }

    // GET AddgroupMemberModal
    public IActionResult AddgroupMemberModal(int groupId)
    {
        return PartialView("AddMemberModalPartialView", groupId);
    }

    // POST AddGroupMembers
    [HttpPost]
    public async Task<IActionResult> AddGroupMembers(int groupId, int userId)
    {
        ResponseVM response = await _groupService.AddGroupMembers(groupId, userId);
        return Json(response);
    }

    // POST RemoveGroupMembers
    [HttpPost]
    public async Task<IActionResult> RemoveGroupMembers(int groupMemberId)
    {
        ResponseVM response = await _groupService.RemoveGroupMembers(groupMemberId);
        int currentUserId = _userService.LoggedInUserId();
        if (groupMemberId == currentUserId && response.Success)
        {
            TempData["successMessage"] = response.Message;
        }
        return Json(response);
    }

    // POST ExportGroups
    [HttpPost]
    public async Task<IActionResult> ExportGroups(FilterVM filter)
    {
        byte[] fileData = await _groupService.ExportGroups(filter);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "groups") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Groups.xlsx");
    }
}
