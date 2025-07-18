using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
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

    #region Non-Group Expenses
    // GET Index
    [Breadcrumb("Non-Group Expenses")]
    [Route("non-group-expenses")]
    public IActionResult Index()
    {
        ViewData["ActiveLink"] = "Non-Group";
        return View("NonGroupExpenses");
    }

    // GET AddNonGroupExpense
    [Breadcrumb("Expense")]
    [Route("non-group-expenses/add")]
    public async Task<IActionResult> AddNonGroupExpense(int expenseId)
    {
        ExpenseVM expense = await _expenseService.GetNonGroupExpense(expenseId);
        ViewData["ActiveLink"] = "Non-Group";
        return View("AddExpense", expense);
    }

    // GET ViewNonGroupExpense
    [Breadcrumb("View")]
    [Route("non-group-expenses/view")]
    public async Task<IActionResult> ViewNonGroupExpense(int expenseId)
    {
        ExpenseVM expense = await _expenseService.GetNonGroupExpense(expenseId);
        expense.IsViewOnly = true;
        ViewData["ActiveLink"] = "Non-Group";
        return View("AddExpense", expense);
    }

    // POST NonGroupExpenseList
    [HttpPost]
    public async Task<IActionResult> NonGroupExpenseList(FilterVM filter)
    {
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter);
        return PartialView("NonGroupExpenseListParialView", expenses);
    }

    // POST ExportNonGroupExpenses
    [HttpPost]
    public async Task<IActionResult> ExportNonGroupExpenses(FilterVM filter)
    {
        byte[] fileData = await _expenseService.ExportExpenses(filter);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "expenses") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Non_Group_Expenses.xlsx");
    }

    #endregion

    #region Group Expenses
    // GET AddGroupExpense
    [Breadcrumb("Expense", FromAction = "Index", FromController = typeof(GroupController))]
    [Route("group-expenses/add")]
    public async Task<IActionResult> AddGroupExpense(int expenseId, int groupId)
    {
        ExpenseVM expense = await _expenseService.GetGroupExpense(expenseId, groupId);
        ViewData["ActiveLink"] = "Groups";
        return View("AddGroupExpense", expense);
    }

    // GET ViewGroupExpense
    [Breadcrumb("Expense", FromAction = "Index", FromController = typeof(GroupController))]
    [Route("group-expenses/view")]
    public async Task<IActionResult> ViewGroupExpense(int expenseId)
    {
        ExpenseVM expense = await _expenseService.GetGroupExpense(expenseId);
        expense.IsViewOnly = true;
        ViewData["ActiveLink"] = "Groups";
        return View("AddGroupExpense", expense);
    }

    // POST ExpensesByGroup
    [HttpPost]
    public async Task<IActionResult> ExpensesByGroup(FilterVM filter, int groupId)
    {
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter, groupId: groupId);
        return PartialView("GroupExpenseListParialView", expenses);
    }

    // POST ExportGroupExpenses
    [HttpPost]
    public async Task<IActionResult> ExportGroupExpenses(FilterVM filter, int groupId = 0)
    {
        byte[] fileData = await _expenseService.ExportExpenses(filter, groupId: groupId);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "expenses") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Group_Expenses.xlsx");
    }
    #endregion

    #region Save Expense
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

    // POST RemoveAttachment
    [HttpPost]
    public async Task<IActionResult> RemoveAttachment(int expenseId)
    {
        ResponseVM response = await _expenseService.RemoveAttachment(expenseId);
        return Json(response);
    }
    #endregion

    #region Friend Expenses
    // POST FriendExpenseList
    [HttpPost]
    public async Task<IActionResult> FriendExpenseList(FilterVM filter, int friendUserId)
    {
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter, friendUserId: friendUserId, isAllExpense: true);
        return PartialView("FriendExpenseListParialView", expenses);
    }

    // POST ExportFriendExpenses
    [HttpPost]
    public async Task<IActionResult> ExportFriendExpenses(FilterVM filter, int friendUserId = 0)
    {
        byte[] fileData = await _expenseService.ExportExpenses(filter, isAllExpense: true, friendUserId: friendUserId);
        if (fileData == null)
        {
            return Json(new ResponseVM { Success = false, Message = NotificationMessages.CanNotExportEmptyList.Replace("{0}", "expenses") });
        }
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Friend_Expenses.xlsx");
    }
    #endregion
}
