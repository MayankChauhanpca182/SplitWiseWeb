using System.Linq.Expressions;
using System.Threading.Tasks;
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
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IExpenseService _expenseService;
    private readonly IGroupService _groupService;
    private readonly IUserService _userService;

    public DashboardService(IExpenseService expenseService, IGenericRepository<FriendRequest> friendRequestRepository, IUserService userService, IGenericRepository<UserReferral> referalRepository, IGenericRepository<ExpenseShare> expenseShareRepository, IGenericRepository<Friend> friendRepository, IGenericRepository<Group> groupRepository, IGenericRepository<Expense> expenseRepository, IGroupService groupService, IGenericRepository<Payment> paymentRepository)
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
        _paymentRepository = paymentRepository;
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

        // Net expense
        dashboard.NetGroupExpense = await NetGroupExpense();
        dashboard.NetNonGroupExpense = await NetNonGroupExpense();
        dashboard.NetExpense = dashboard.NetGroupExpense + dashboard.NetNonGroupExpense;

        // Total paid
        dashboard.TotalPaid = await TotalPaid();

        // Total settled
        dashboard.TotalSettled = await TotalSettled();

        // Recent expenses
        FilterVM filter = new FilterVM
        {
            SortColumn = "date",
            SortOrder = "desc"
        };
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.ExpenseList(filter, isAllExpense: true);
        dashboard.RecentExpenses = expenses.List.ToList();

        // Recent payments
        dashboard.RecentPayments = await GetPayments();

        return dashboard;
    }

    private async Task<decimal> NetGroupExpense()
    {
        int currentUserId = _userService.LoggedInUserId();

        decimal netAmount = await _expenseShareRepository.Sum(
            selector: es => es.Expense.PaidById == currentUserId ? (es.ShareAmount - es.SettledAmount) : -(es.ShareAmount - es.SettledAmount),
            predicate: es => es.DeletedAt == null
                        && es.Expense.DeletedAt == null && es.Expense.GroupId != null
                        && es.UserId != es.Expense.PaidById
                        && (es.Expense.PaidById == currentUserId || es.UserId == currentUserId),
            includes: new List<Expression<Func<ExpenseShare, object>>>
            {
                    es => es.Expense
            }
        );

        return netAmount;
    }

    private async Task<decimal> NetNonGroupExpense()
    {
        int currentUserId = _userService.LoggedInUserId();

        decimal netAmount = await _expenseShareRepository.Sum(
            selector: es => es.Expense.PaidById == currentUserId ? (es.ShareAmount - es.SettledAmount) : -(es.ShareAmount - es.SettledAmount),
            predicate: es => es.DeletedAt == null
                        && es.Expense.DeletedAt == null && es.Expense.GroupId == null
                        && es.UserId != es.Expense.PaidById
                        && (es.Expense.PaidById == currentUserId || es.UserId == currentUserId),
            includes: new List<Expression<Func<ExpenseShare, object>>>
            {
                    es => es.Expense
            }
        );

        return netAmount;
    }

    private async Task<List<Payment>> GetPayments()
    {
        int currentUserId = _userService.LoggedInUserId();

        PaginatedItemsVM<Payment> payments = await _paymentRepository.PaginatedList(
            predicate: p => p.DeletedAt == null && (p.PaidById == currentUserId || p.PaidToId == currentUserId),
            orderBy: p => p.OrderByDescending(p => p.Id),
            includes: new List<Expression<Func<Payment, object>>>
            {
                p => p.PaidByUser,
                p => p.PaidToUser
            },
            pageNumber: 1,
            pageSize: 5
        );
        return payments.Items.ToList();
    }

    private async Task<decimal> TotalPaid()
    {
        int currentUserId = _userService.LoggedInUserId();

        decimal totalSpent = await _expenseRepository.Sum(
            selector: e => e.Amount,
            predicate: e => e.DeletedAt == null && e.PaidById == currentUserId
        );

        return totalSpent;
    }

    private async Task<decimal> TotalSettled()
    {
        int currentUserId = _userService.LoggedInUserId();

        decimal totalPaid = await _paymentRepository.Sum(
            selector: p => p.Amount,
            predicate: p => p.PaidById == currentUserId
        );

        return totalPaid;
    }
}
