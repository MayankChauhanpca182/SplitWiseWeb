using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
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
    [Breadcrumb("Settlement")]
    [Route("settlement")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Settlement";
        return View();
    }

    // POST SettlementList
    [HttpPost]
    public async Task<IActionResult> SettlementList(FilterVM filter)
    {
        PaginatedListVM<SettlementListVM> paginatedList = await _settlementService.SettlementList(filter);
        return PartialView("SettlementListPartialView", paginatedList);
    }

    // POST ExportSettlements
    [HttpPost]
    public async Task<IActionResult> ExportSettlements(FilterVM filter)
    {
        byte[] fileData = await _settlementService.ExportSettlements(filter);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "settlements") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Settlements.xlsx");
    }

    [Breadcrumb("Settle Up")]
    [Route("settle-up/{friendUserId}")]
    public async Task<IActionResult> SettleUp(int friendUserId)
    {
        User friendUser = await _userService.GetById(friendUserId);
        if (friendUser == null)
        {
            throw new KeyNotFoundException(NotificationMessages.NotFound.Replace("{0}", "user"));
        }

        ViewData["ActiveLink"] = "Settlement";
        return View(friendUser);
    }

    // GET SettleUpList
    public async Task<IActionResult> SettleUpList(int friendUserId)
    {
        SettleUpListVM settleUpList = await _settlementService.SettleUpList(friendUserId);
        return PartialView("SettleUpList", settleUpList);
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
