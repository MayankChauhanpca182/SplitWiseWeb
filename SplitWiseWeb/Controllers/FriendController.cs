using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class FriendController : Controller
{
    private readonly IFriendService _freiendService;
    private readonly IUserService _userService;

    public FriendController(IFriendService freiendService, IUserService userService)
    {
        _freiendService = freiendService;
        _userService = userService;
    }

    // GET Index
    [Breadcrumb("Friends")]
    [Route("friends")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Friends";
        return View();
    }

    // POST FriendList
    [HttpPost]
    public async Task<IActionResult> FriendList(FilterVM filter)
    {
        FriendListVM friendList = await _freiendService.FriendList(filter);
        return PartialView("_FriendListPartialView", friendList);
    }

    // GET AddFriendModal
    public IActionResult AddFriendModal()
    {
        return PartialView("_AddFriendModalPartialView", new FriendRequestVM());
    }

    // POST SendFriendRequest
    [HttpPost]
    public async Task<IActionResult> SendFriendRequest(FriendRequestVM friendRequest)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_AddFriendModalPartialView", friendRequest);
        }

        ResponseVM response = await _freiendService.CheckExisitngFrindship(friendRequest.Email);

        if (!response.Success)
        {
            return Json(response);
        }

        response = await _freiendService.SendRequest(friendRequest);
        if (response.Success)
        {
            return Json(response);
        }
        else
        {
            friendRequest.Message = response.Message;
            return PartialView("_FriendReferralModalParialView", friendRequest);
        }
    }

    // POST SendReferral
    [HttpPost]
    public async Task<IActionResult> SendReferral(FriendRequestVM friendRequest)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_FriendReferralModalParialView", friendRequest);
        }

        ResponseVM response = await _freiendService.SendReferral(friendRequest);
        return Json(response);
    }

    // GET FriendRequests
    [Breadcrumb("Friend Requests", FromController = typeof(DashboardController))]
    [Route("friendRequests")]
    public IActionResult FriendRequests()
    {
        ViewData["ActiveLink"] = "Friends";
        return View();
    }

    // POST FriendList
    [HttpPost]
    public async Task<IActionResult> FriendRequestList(FilterVM filter)
    {
        FriendRequestListVM friendRequestList = await _freiendService.FriendRequestList(filter);
        return PartialView("_FriendRequestListPartialView", friendRequestList);
    }

    // POST AcceptRequest
    [HttpPost]
    public async Task<IActionResult> AcceptRequest(int id)
    {
        ResponseVM response = await _freiendService.AcceptRequest(id);
        return Json(response);
    }

    // POST RejectRequest
    [HttpPost]
    public async Task<IActionResult> RejectRequest(int id)
    {
        ResponseVM response = await _freiendService.RejectRequest(id);
        return Json(response);
    }

    // POST RemoveFriend
    [HttpPost]
    public async Task<IActionResult> RemoveFriend(int friendId)
    {
        ResponseVM response = await _freiendService.RemoveFriend(friendId);
        return Json(response);
    }

}
