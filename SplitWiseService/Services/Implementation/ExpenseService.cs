using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class ExpenseService : IExpenseService
{
    private readonly IGenericRepository<Expense> _expenseRepository;
    private readonly ICategoryService _categoryService;
    private readonly ICommonService _commonService;
    private readonly IGroupService _groupService;

    public ExpenseService(IGenericRepository<Expense> expenseRepository, ICategoryService categoryService, ICommonService commonService, IGroupService groupService)
    {
        _expenseRepository = expenseRepository;
        _categoryService = categoryService;
        _commonService = commonService;
        _groupService = groupService;

    }

    public async Task<ExpenseVM> GetExpense(int? groupId, int expenseId = 0)
    {
        ExpenseVM expenseVM = new ExpenseVM();
        if (expenseId > 0)
        {
            Expense expense = await _expenseRepository.Get(
                predicate: e => e.Id == expenseId,
                includes: new List<Expression<Func<Expense, object>>>
                {
                    e => e.ExpenseShares
                },
                thenIncludes: new List<Func<IQueryable<Expense>, IQueryable<Expense>>>
                {
                    q => q.Include(e => e.ExpenseShares)
                        .ThenInclude(es => es.User)
                }
            );
            expenseVM.Id = expense.Id;
            expenseVM.GroupId = expense.GroupId;
            expenseVM.Title = expense.Title;
            expenseVM.Amount = expense.Amount;
            expenseVM.CategoryId = expense.ExpenseCategoryId;
            expenseVM.CurrencyId = expense.CurrencyId;
            expenseVM.PaidById = expense.PaidById;
            expenseVM.PaymentDate = expense.PaidDate;
            expenseVM.SplitType = expense.SplitType;

            expenseVM.ExpenseShares = expense.ExpenseShares
                    .Select(es => new ExpenseShareVM
                    {
                        Id = es.Id,
                        UserId = es.UserId,
                        ShareAmount = es.ShareAmount,
                        UserName = $"{es.User.FirstName} {es.User.LastName}",
                        ProfileImagePath = es.User.ProfileImagePath
                    }).ToList();
        }
        else if (groupId != null)
        {
            // expenseVM.ExpenseShares = await _groupService.GetMembers((int)groupId);
        }

        expenseVM.Categories = await _categoryService.GetList(groupId);
        expenseVM.Currencies = await _commonService.CurrencyList();
        return expenseVM;
    }

}
