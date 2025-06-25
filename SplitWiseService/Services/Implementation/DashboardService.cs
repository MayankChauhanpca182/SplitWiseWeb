using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class DashboardService : IDashboardService
{
    private readonly IGenericRepository<FriendRequest> _friendRequestRepository;
    private readonly IGenericRepository<UserReferral> _referalRepository;
    private readonly IGenericRepository<ExpenseShare> _expenseShareRepository;
    private readonly IFriendService _friendService;
    private readonly IGroupService _groupService;
    private readonly IExpenseService _expenseService;
    private readonly IUserService _userService;

    public DashboardService(IFriendService friendService, IGroupService groupService, IExpenseService expenseService, IGenericRepository<FriendRequest> friendRequestRepository, IUserService userService, IGenericRepository<UserReferral> referalRepository, IGenericRepository<ExpenseShare> expenseShareRepository)
    {
        _friendService = friendService;
        _groupService = groupService;
        _expenseService = expenseService;
        _friendRequestRepository = friendRequestRepository;
        _userService = userService;
        _referalRepository = referalRepository;
        _expenseShareRepository = expenseShareRepository;
    }

    public async Task<DashboardVM> GetDashboard()
    {
        User currentUser = await _userService.LoggedInUser();
        DashboardVM dashboard = new DashboardVM();
        dashboard.FriendsAccepted = await _friendService.FriendsCount();
        dashboard.FriendsPending = await _friendRequestRepository.Count(
            predicate: fr => fr.ReceiverId == currentUser.Id && fr.Status == FeriendRequestStatus.Requested
        );
        dashboard.FriendsRequested = await _friendRequestRepository.Count(
            predicate: fr => fr.RequesterId == currentUser.Id && fr.Status == FeriendRequestStatus.Requested
        );
        dashboard.FriendsReferred = await _referalRepository.Count(
            predicate: ur => ur.ReferredFromUserId == currentUser.Id
        );


        dashboard.GroupsCount = await _groupService.GroupCount();
        dashboard.TotalExpense = await TotalExpense();
        FilterVM filter = new FilterVM
        {
            SortColumn = "date",
            SortOrder = "desc"
        };
        PaginatedListVM<ExpenseVM> expenses = await _expenseService.IndividualList(filter);
        dashboard.RecentExpenses = expenses.List.ToList();
        return dashboard;
    }

    public async Task<decimal> TotalExpense()
    {
        int currentUserId = _userService.LoggedInUserId();
        decimal totalExpense = await _expenseShareRepository.Sum(
            selector: e => e.ShareAmount,
            predicate: e => e.DeletedAt == null && e.UserId == currentUserId
        );
        return totalExpense;
    }

}
