using System.Linq.Expressions;
using Org.BouncyCastle.Crypto.Engines;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class DashboardService : IDashboardService
{
    private readonly IGenericRepository<Friend> _friendRepository;
    private readonly IGenericRepository<FriendRequest> _friendRequestRepository;
    private readonly IGenericRepository<UserReferral> _referalRepository;
    private readonly IGenericRepository<ExpenseShare> _expenseShareRepository;
    private readonly IGenericRepository<Group> _groupRepository;
    private readonly IGenericRepository<Expense> _expenseRepository;
    private readonly IExpenseService _expenseService;
    private readonly IGroupService _groupService;
    private readonly IUserService _userService;

    public DashboardService(IExpenseService expenseService, IGenericRepository<FriendRequest> friendRequestRepository, IUserService userService, IGenericRepository<UserReferral> referalRepository, IGenericRepository<ExpenseShare> expenseShareRepository, IGenericRepository<Friend> friendRepository, IGenericRepository<Group> groupRepository, IGenericRepository<Expense> expenseRepository, IGroupService groupService)
    {
        _expenseService = expenseService;
        _friendRequestRepository = friendRequestRepository;
        _userService = userService;
        _referalRepository = referalRepository;
        _expenseShareRepository = expenseShareRepository;
        _friendRepository = friendRepository;
        _groupRepository = groupRepository;
        _expenseRepository = expenseRepository;
        _groupService = groupService;
    }

    public async Task<DashboardVM> GetDashboard()
    {
        User currentUser = await _userService.LoggedInUser();
        DashboardVM dashboard = new DashboardVM();

        // Friends
        dashboard.FriendsAccepted = await _friendRepository.Count(
            predicate: f => f.DeletedAt == null && (f.Friend1 == currentUser.Id || f.Friend2 == currentUser.Id)
        );
        dashboard.FriendsPending = await _friendRequestRepository.Count(
            predicate: fr => fr.ReceiverId == currentUser.Id && fr.Status == FeriendRequestStatus.Requested
        );
        dashboard.FriendsRequested = await _friendRequestRepository.Count(
            predicate: fr => fr.RequesterId == currentUser.Id && fr.Status == FeriendRequestStatus.Requested
        );
        dashboard.FriendsReferred = await _referalRepository.Count(
            predicate: ur => ur.ReferredFromUserId == currentUser.Id
        );

        // Groups
        dashboard.GroupsCount = await _groupRepository.Count(
            predicate: g => g.DeletedAt == null && g.GroupMembers.Any(gm => gm.UserId == currentUser.Id && gm.DeletedAt == null),
            includes: new List<Expression<Func<Group, object>>>
            {
                g => g.GroupMembers
            }
        );
        // dashboard.TotalGroupExpense = await TotalGroupExpense();
        dashboard.TotalGroupExpense = await NetGroupExpense();

        // Expenses
        dashboard.TotalExpense = await NetBalance();

        FilterVM filter = new FilterVM
        {
            SortColumn = "date",
            SortOrder = "desc"
        };
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter, isAllExpense: true);
        dashboard.RecentExpenses = expenses.List.ToList();
        return dashboard;
    }

    private async Task<decimal> NetBalance()
    {
        int currentUserId = _userService.LoggedInUserId();

        decimal youAreOwed = await _expenseShareRepository.Sum(
            selector: es => es.ShareAmount,
            predicate: es => es.DeletedAt == null && es.Expense.PaidById == currentUserId && es.UserId != currentUserId,
            includes: new List<Expression<Func<ExpenseShare, object>>>
            {
                es => es.Expense
            }
        );

        decimal youOweOthers = await _expenseShareRepository.Sum(
            selector: es => es.ShareAmount,
            predicate: es => es.DeletedAt == null && es.Expense.PaidById != currentUserId && es.UserId == currentUserId,
            includes: new List<Expression<Func<ExpenseShare, object>>>
            {
                es => es.Expense
            }
        );

        return youAreOwed - youOweOthers;
    }

    private async Task<decimal> TotalGroupExpense()
    {
        int currentUserId = _userService.LoggedInUserId();

        decimal total = await _expenseShareRepository.Sum(
            selector: es => es.ShareAmount,
            predicate: es => es.DeletedAt == null && es.UserId == currentUserId
                    && es.Expense.DeletedAt == null
                    && es.Expense.GroupId != null,
            includes: new List<Expression<Func<ExpenseShare, object>>>
            {
                es => es.Expense
            }
        );
        return total;
    }

    private async Task<decimal> NetGroupExpense()
    {
        int currentUserId = _userService.LoggedInUserId();

        List<GroupVM> list = _groupService.GroupList(new FilterVM { PageNumber = 0, PageSize = 0 }).Result.List.ToList();

        decimal total = list.Sum(g => g.Expense);

        return total;
    }
}
