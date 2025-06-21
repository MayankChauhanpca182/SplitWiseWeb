using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
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
    public async Task<IActionResult> AddGroup(int groupId = 0)
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

    // GET GroupDetails
    [Route("groupDetails/{groupId}")]
    [Breadcrumb("Group Details", FromAction = "Index")]
    public async Task<IActionResult> GroupDetails(int groupId)
    {
        GroupVM group = await _groupService.GetGroup(groupId);
        group.Members = await _groupService.GetMembers(groupId);
        return PartialView("GroupDetails", group);
    }

    // GET GroupMembers
    public async Task<IActionResult> GroupMembers(int groupId)
    {
        List<GroupMemberVM> members = await _groupService.GetMembers(groupId);
        return PartialView("GroupMembersPartialView", members);
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
        return Json(response);
    }
}
