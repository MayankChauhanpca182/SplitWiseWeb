using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class ExpenseController : Controller
{
    public ExpenseController()
    {
    }

    // GET Index
    [Breadcrumb("Expenses")]
    [Route("expenses")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Expenses";
        return View();
    }

    // GET AddExpensePage
    [Breadcrumb("Expense Details")]
    [Route("expenseDetails")]
    public IActionResult AddExpense(int expenseId, int groupId)
    {
        ViewData["ActiveLink"] = "Expense Details";
        return View();
    }

    // POST SaveExpense
    public IActionResult SaveExpense()
    {
        return View("AddExpense");
    }

}
