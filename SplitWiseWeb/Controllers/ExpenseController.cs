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
    [Breadcrumb("Individual Expenses")]
    [Route("individual-expenses")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Individual Expenses";
        return View("IndividialExpenses");
    }

    // GET GroupExpenses
    [Breadcrumb("Group Expenses", FromController = typeof(DashboardController))]
    [Route("group-expenses")]
    public IActionResult GroupExpenses()
    {
        ViewData["ActiveLink"] = "Group Expenses";
        return View();
    }

    // GET AddIndividualExpense
    [Breadcrumb("Details")]
    [Route("individual-expenses/add")]
    public async Task<IActionResult> AddIndividualExpense(int expenseId)
    {
        ExpenseVM expense = await _expenseService.GetIndividualExpense(expenseId);
        ViewData["ActiveLink"] = "Individual Expenses";
        return View("AddExpense", expense);
    }

    // GET AddGroupExpense
    [Breadcrumb("Details", FromAction = "GroupExpenses")]
    [Route("group-expenses/add")]
    public async Task<IActionResult> AddGroupExpense(int expenseId, int groupId)
    {
        ExpenseVM expense = await _expenseService.GetGroupExpense(expenseId, groupId);
        ViewData["ActiveLink"] = "Group Expenses";
        return View("AddGroupExpense", expense);
    }

    // GET ViewIndividualExpense
    [Breadcrumb("View")]
    [Route("individual-expenses/view")]
    public async Task<IActionResult> ViewIndividualExpense(int expenseId)
    {
        ExpenseVM expense = await _expenseService.GetIndividualExpense(expenseId);
        expense.IsViewOnly = true;
        ViewData["ActiveLink"] = "Individual Expenses";
        return View("AddExpense", expense);
    }

    // GET ViewGroupExpense
    [Breadcrumb("View", FromAction = "GroupExpenses")]
    [Route("group-expenses/view")]
    public async Task<IActionResult> ViewGroupExpense(int expenseId)
    {
        ExpenseVM expense = await _expenseService.GetGroupExpense(expenseId);
        expense.IsViewOnly = true;
        ViewData["ActiveLink"] = "Group Expenses";
        return View("AddGroupExpense", expense);
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
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter);
        return PartialView("IndividualExpenseListParialView", expenses);
    }

    // POST GroupExpenseList
    [HttpPost]
    public async Task<IActionResult> GroupExpenseList(FilterVM filter)
    {
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter, isGroupExpenses: true);
        return PartialView("GroupExpenseListParialView", expenses);
    }

    // POST ExpensesByGroup
    public async Task<IActionResult> ExpensesByGroup(FilterVM filter, int groupId)
    {
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter, isGroupExpenses: true, groupId: groupId);
        return PartialView("GroupExpenseListParialView", expenses);
    }

}
