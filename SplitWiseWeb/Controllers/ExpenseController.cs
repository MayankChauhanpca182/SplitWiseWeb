using System.Text.Json;
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

    // GET AddExpense
    [Breadcrumb("Add")]
    [Route("expense/add")]
    public async Task<IActionResult> AddExpense(int expenseId, int groupId)
    {
        ExpenseVM expense = await _expenseService.GetExpense(expenseId, groupId);
        ViewData["ActiveLink"] = "Expenses";
        return View(expense);
    }

    // GET ViewExpense
    [Breadcrumb("View")]
    [Route("expense/view")]
    public async Task<IActionResult> ViewExpense(int expenseId, int groupId)
    {
        ExpenseVM expense = await _expenseService.GetExpense(expenseId, groupId);
        expense.IsViewOnly = true;
        ViewData["ActiveLink"] = "Expenses";
        return View("AddExpense", expense);
    }

    // POST SaveExpense
    [HttpPost]
    public async Task<IActionResult> SaveExpense(ExpenseVM newExpense, string expenseMembersJson)
    {
        if (!string.IsNullOrEmpty(expenseMembersJson))
        {
            newExpense.ExpenseShares = JsonSerializer.Deserialize<List<ExpenseShareVM>>(expenseMembersJson);
        }

        ResponseVM response = await _expenseService.SaveExpense(newExpense);
        if (response.Success)
        {
            TempData["successMessage"] = response.Message;
        }
        return Json(response);
    }

    // POST IndividualExpenseList
    [HttpPost]
    public async Task<IActionResult> IndividualExpenseList(FilterVM filter)
    {
        PaginatedListVM<ExpenseVM> expense = await _expenseService.IndividualList(filter);
        return PartialView("IndividualExpenseListParialView", expense);
    }
}
