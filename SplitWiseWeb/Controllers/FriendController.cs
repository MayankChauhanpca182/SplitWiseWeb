using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

// [Authorize]
[Breadcrumb("Friends")]
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
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Friends";
        return View();
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

        ResponseVM response = new ResponseVM();

        User currentUser = await _userService.LoggedInUser();
        if (friendRequest.Email.ToLower() == currentUser.EmailAddress.ToLower())
        {
            response.Success = false;
            response.Message = NotificationMessages.FriendRequestToSelf;
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
    [Breadcrumb("Friend Requests", FromAction = "Index")]
    public IActionResult FriendRequests()
    {
        return View();
    }

    // POST FriendList
    [HttpPost]
    public async Task<IActionResult> FriendList(FilterVM filter)
    {
        FriendRequestListVM friendRequestList = await _freiendService.FriendRequestList(filter);
        return PartialView("_FriendRequestListPartialView", friendRequestList);
    }
}
