using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // POST FriendPayments
    public async Task<IActionResult> FriendPaymentList(FilterVM filter, int friendUserId)
    {
        PaginatedListVM<Payment> payments = await _paymentService.FriendPaymentList(filter, friendUserId);
        return PartialView("PaymentListPartialView", payments);
    }

}
