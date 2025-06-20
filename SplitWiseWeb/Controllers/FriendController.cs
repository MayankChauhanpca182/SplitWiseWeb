using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class FriendController : Controller
{
    private readonly IFriendService _friendService;

    public FriendController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    // GET Index
    [Breadcrumb("Friends")]
    [Route("friends")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    // POST FriendList
    [HttpPost]
    public async Task<IActionResult> FriendList(FilterVM filter)
    {
        PaginatedListVM<FriendVM> paginatedList = await _friendService.FriendList(filter);
        return PartialView("FriendListPartialView", paginatedList);
    }

    // GET AddFriendModal
    public IActionResult AddFriendModal()
    {
        return PartialView("AddFriendModalPartialView", new FriendRequestVM());
    }

    // POST SendFriendRequest
    [HttpPost]
    public async Task<IActionResult> SendFriendRequest(FriendRequestVM friendRequest)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("AddFriendModalPartialView", friendRequest);
        }

        ResponseVM response = await _friendService.CheckExisitngFrindship(friendRequest.Email);
        if (!response.Success)
        {
            return Json(response);
        }

        response = await _friendService.SendRequest(friendRequest.Email);
        if (!response.ShowNextAction)
        {
            return Json(response);
        }
        else
        {
            friendRequest.Message = response.Message;
            return PartialView("FriendReferralModalParialView", friendRequest);
        }
    }

    // POST SendFriendRequestAjax
    [HttpPost]
    public async Task<IActionResult> SendFriendRequestAjax(string email)
    {
        ResponseVM response = await _friendService.SendRequest(email);
        return Json(response);
    }

    // POST SendReferral
    [HttpPost]
    public async Task<IActionResult> SendReferral(FriendRequestVM friendRequest)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("FriendReferralModalParialView", friendRequest);
        }

        ResponseVM response = await _friendService.SendReferral(friendRequest);
        return Json(response);
    }

    // GET FriendRequests
    [Breadcrumb("Friend Requests", FromController = typeof(DashboardController))]
    [Route("friendRequests")]
    public IActionResult FriendRequests()
    {
        return PartialView("FriendRequests");
    }

    // POST FriendRequestList
    [HttpPost]
    public async Task<IActionResult> FriendRequestList(FilterVM filter)
    {
        PaginatedListVM<FriendRequestVM> paginatedList = await _friendService.FriendRequestList(filter);
        return PartialView("FriendRequestListPartialView", paginatedList);
    }

    // POST AcceptRequest
    [HttpPost]
    public async Task<IActionResult> AcceptRequest(int id)
    {
        ResponseVM response = await _friendService.AcceptRequest(id);
        return Json(response);
    }

    // POST RejectRequest
    [HttpPost]
    public async Task<IActionResult> RejectRequest(int id)
    {
        ResponseVM response = await _friendService.RejectRequest(id);
        return Json(response);
    }

    // POST RemoveFriend
    [HttpPost]
    public async Task<IActionResult> RemoveFriend(int friendId)
    {
        ResponseVM response = await _friendService.RemoveFriend(friendId);
        return Json(response);
    }

    // POST ExportFriends
    [HttpPost]
    public async Task<IActionResult> ExportFriends(FilterVM filter)
    {
        byte[] fileData = await _friendService.ExportFriends(filter);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "friend") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Friends.xlsx");
    }

    // POST FriengListForGroup
    [HttpPost]
    public async Task<IActionResult> FriengListForGroup(FilterVM filter, int groupId)
    {
        PaginatedListVM<FriendVM> paginatedList = await _friendService.FriendList(filter, groupId);
        return PartialView("FriendListForGroupPartialView", paginatedList);
    }
}
