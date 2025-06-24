using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class ExpenseController : Controller
{
    private readonly IExpenseService _expenseService;
    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
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
    [Breadcrumb("Add")]
    [Route("expense/add")]
    public async Task<IActionResult> AddExpense(int expenseId, int groupId)
    {
        ExpenseVM expense = await _expenseService.GetExpense(expenseId, groupId);   
        ViewData["ActiveLink"] = "Expenses";
        return View(expense);
    }

    // POST SaveExpense
    public IActionResult SaveExpense(ExpenseVM newExpense)
    {
        return View("AddExpense");
    }

}
