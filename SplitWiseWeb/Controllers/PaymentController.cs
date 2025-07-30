using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // POST FriendPayments
    [HttpPost]
    public async Task<IActionResult> FriendPaymentList(FilterVM filter, int friendUserId)
    {
        PaginatedListVM<Payment> payments = await _paymentService.FriendPaymentList(filter, friendUserId);
        return PartialView("PaymentListPartialView", payments);
    }

    // POST ExportFriendExpenses
    [HttpPost]
    public async Task<IActionResult> ExportPayments(FilterVM filter, int friendUserId = 0)
    {
        byte[] fileData = await _paymentService.ExportPayments(filter, friendUserId: friendUserId);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "payments") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Payments.xlsx");
    }

}
