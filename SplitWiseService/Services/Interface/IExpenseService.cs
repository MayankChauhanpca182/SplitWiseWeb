using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IExpenseService
{
    public Task<ExpenseVM> GetIndividualExpense(int expenseId = 0);
    public Task<ExpenseVM> GetGroupExpense(int expenseId = 0, int groupId = 0);
    public Task<ResponseVM> SaveExpense(ExpenseVM newExpense);
    public Task<PaginatedListVM<ExpenseVM>> ExpenseList(FilterVM filter, bool isAllExpense = false, bool isGroupExpenses = false, int groupId = 0);
}
