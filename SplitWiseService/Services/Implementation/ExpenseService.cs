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
    private readonly IUserService _userService;
    private readonly IFriendService _friendService;

    public ExpenseService(IGenericRepository<Expense> expenseRepository, ICategoryService categoryService, ICommonService commonService, IGroupService groupService, IUserService userService, IFriendService friendService)
    {
        _expenseRepository = expenseRepository;
        _categoryService = categoryService;
        _commonService = commonService;
        _groupService = groupService;
        _userService = userService;
        _friendService = friendService;

    }

    public async Task<ExpenseVM> GetExpense(int expenseId = 0, int groupId = 0)
    {
        ExpenseVM expenseVM = new ExpenseVM();
        User currentUser = await _userService.LoggedInUser();

        if (expenseId > 0)
        {
            // Expense expense = await _expenseRepository.Get(
            //     predicate: e => e.Id == expenseId,
            //     includes: new List<Expression<Func<Expense, object>>>
            //     {
            //         e => e.ExpenseShares
            //     },
            //     thenIncludes: new List<Func<IQueryable<Expense>, IQueryable<Expense>>>
            //     {
            //         q => q.Include(e => e.ExpenseShares)
            //             .ThenInclude(es => es.User)
            //     }
            // );
            // expenseVM.Id = expense.Id;
            // expenseVM.GroupId = expense.GroupId;
            // expenseVM.Title = expense.Title;
            // expenseVM.Amount = expense.Amount;
            // expenseVM.CategoryId = expense.ExpenseCategoryId;
            // expenseVM.CurrencyId = expense.CurrencyId;
            // expenseVM.PaidById = expense.PaidById;
            // expenseVM.PaymentDate = expense.PaidDate;
            // expenseVM.SplitType = expense.SplitType;

            // expenseVM.ExpenseShares = expense.ExpenseShares
            //         .Select(es => new ExpenseShareVM
            //         {
            //             Id = es.Id,
            //             UserId = es.UserId,
            //             ShareAmount = es.ShareAmount,
            //             UserName = $"{es.User.FirstName} {es.User.LastName}",
            //             ProfileImagePath = es.User.ProfileImagePath
            //         }).ToList();
        }
        else if (groupId != null && groupId > 0)
        {
            // expenseVM.ExpenseShares = await _groupService.GetMembers((int)groupId);
        }
        else
        {
            expenseVM.Friends = _friendService.FriendList(new FilterVM { PageNumber = 0, PageSize = 0 }).Result.List.ToList();
        }

        expenseVM.CurrentUser = currentUser;
        expenseVM.Friends.Add(new FriendVM
        {
            UserId = currentUser.Id,
            Name = $"{currentUser.FirstName} {currentUser.LastName}",
            ProfileImagePath = currentUser.ProfileImagePath
        });
        expenseVM.Categories = await _categoryService.GetList();
        expenseVM.Currencies = await _commonService.CurrencyList();
        return expenseVM;
    }

}
