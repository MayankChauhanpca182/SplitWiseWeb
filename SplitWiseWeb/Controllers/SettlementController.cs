using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
[Breadcrumb("Settlement")]
public class SettlementController : Controller
{
    private readonly ISettlementService _settlementService;
    private readonly IUserService _userService;
    private readonly ICommonService _commonService;
    private readonly IFriendService _friendService;

    public SettlementController(ISettlementService settlementService, IUserService userService, ICommonService commonService, IFriendService friendService)
    {
        _settlementService = settlementService;
        _userService = userService;
        _commonService = commonService;
        _friendService = friendService;
    }

    // GET Index
    [Route("settlement")]
    public async Task<IActionResult> Index(int friendUserId)
    {
        int currentUserId = _userService.LoggedInUserId();
        Friend friend = await _friendService.GetFriend(currentUserId, friendUserId);
        User friendUser = friend.Friend1 == currentUserId ? friend.Friend2UserNavigation : friend.Friend1UserNavigation;
        return View(friendUser);
    }

    // GET SettlementList
    public async Task<IActionResult> SettlementList(int friendUserId)
    {
        SettlementListVM settlementList = await _settlementService.GetList(friendUserId);
        return PartialView("SettlementList", settlementList);
    }

    // POST SettlementModal
    [HttpPost]
    public async Task<IActionResult> SettlementModal(decimal amount, int groupId, int friendUserId, bool settleAll)
    {
        SettlementVM settlement = new SettlementVM
        {
            GroupId = groupId,
            PaidById = _userService.LoggedInUserId(),
            PaidToId = friendUserId,
            PaidToUser = await _userService.GetById(friendUserId),
            Amount = amount,
            Currencies = await _commonService.CurrencyList(),
            SettleAll = settleAll
        };
        return PartialView("SettlementModalPartialView", settlement);
    }

    // POST AddSettlement
    [HttpPost]
    public async Task<IActionResult> AddSettlement(SettlementVM settlement)
    {
        if (!ModelState.IsValid)
        {
            settlement.PaidToUser = await _userService.GetById(settlement.PaidToId);
            settlement.Currencies = await _commonService.CurrencyList();
            return PartialView("SettlementModalPartialView", settlement);
        }
        ResponseVM response = await _settlementService.AddSettlement(settlement);
        return Json(response);
    }

}
