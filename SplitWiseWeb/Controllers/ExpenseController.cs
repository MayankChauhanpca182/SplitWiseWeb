using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.ViewModels;

namespace SplitWiseWeb.Controllers;

public class ExpenseController : Controller
{
    public ExpenseController()
    {
    }

    // GET AddExpenseModal
    public IActionResult AddExpenseModal(int expenseId = 0, int groupId = 0)
    {
        ExpenseVM expense = new ExpenseVM();
        if (expenseId > 0)
        {
            // Fetch expense details
        }
        return PartialView("AddExpenseModalPartialView", expense);
    }

}
