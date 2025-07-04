using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
[Breadcrumb("Settlement")]
public class SettlementController : Controller
{
    private readonly ISettlementService _settlementService;

    public SettlementController(ISettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    // GET Index
    [Route("settlement")]
    public async Task<IActionResult> Index(int friendUserId)
    {
        SettlementListVM settlementList = await _settlementService.GetList(friendUserId);
        return View("SettlementList", settlementList);
    }

}
