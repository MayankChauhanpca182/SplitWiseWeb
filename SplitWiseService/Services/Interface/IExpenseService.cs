using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IExpenseService
{
    public Task<ExpenseVM> GetExpense(int expenseId = 0, int groupId = 0);
    public Task<ResponseVM> SaveExpense(ExpenseVM newExpense);
    public Task<PaginatedListVM<ExpenseVM>> ExpenseList(FilterVM filter, bool isAllExpense = false, bool isGroupExpenses = false);
}
