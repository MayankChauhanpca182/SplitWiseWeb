using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class ActivityService : IActivityService
{
    private readonly IGenericRepository<Activity> _activityRepository;
    private readonly IUserService _userService;

    public ActivityService(IGenericRepository<Activity> activityRepository, IUserService userService)
    {
        _activityRepository = activityRepository;
        _userService = userService;
    }

    public async Task AddActivity(ActivityType activityType, int? groupId = null, int? expenseId = null, int? paymentId = null, int? performedOnId = null)
    {
        int currentUserId = _userService.LoggedInUserId();

        Activity activity = new Activity
        {
            ActivityType = activityType,
            PerformedById = currentUserId,
            PerformedOnId = performedOnId,
            GroupId = groupId,
            ExpenseId = expenseId,
            PaymentId = paymentId,
            CreatedById = currentUserId,
            UpdatedAt = DateTime.Now,
            UpdatedById = currentUserId
        };

        await _activityRepository.Add(activity);
        return;
    }

    public async Task<List<Activity>> GroupActivityList(int groupId)
    {
        List<Activity> groupActivities = await _activityRepository.List(
            predicate: a => a.DeletedAt == null && a.GroupId == groupId,
            orderBy: a => a.OrderByDescending(a => a.CreatedAt),
            includes: new List<System.Linq.Expressions.Expression<Func<Activity, object>>>
            {
                a => a.Group,
                a => a.PerformedByUser,
                a => a.PerformedOnUser,
                a => a.Expense,
                a => a.Payment
            }
        );

        return groupActivities;
    }

    public async Task<List<Activity>> UserActivityList()
    {
        int currentUserId = _userService.LoggedInUserId();

        List<Activity> userActivities = await _activityRepository.List(
            predicate: a => a.DeletedAt == null && (a.PerformedById == currentUserId || a.PerformedOnId == currentUserId),
            orderBy: a => a.OrderByDescending(a => a.CreatedAt),
            includes: new List<System.Linq.Expressions.Expression<Func<Activity, object>>>
            {
                a => a.Group,
                a => a.PerformedByUser,
                a => a.PerformedOnUser,
                a => a.Expense,
                a => a.Payment
            }
        );

        return userActivities;
    }

    public async Task<List<Activity>> ActivityList(FilterVM filter, int? groupId = null)
    {
        int currentUserId = _userService.LoggedInUserId();
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? string.Empty : filter.SearchString.Replace(" ", "").ToLower();

        bool isSearchYou = "you".Contains(searchString);

        PaginatedItemsVM<Activity> userActivities = await _activityRepository.PaginatedList(
            predicate: a => a.DeletedAt == null
                            && (groupId != null ? a.GroupId == groupId : (a.PerformedById == currentUserId || a.PerformedOnId == currentUserId))
                            && a.CreatedAt >= filter.FromDate
                            && a.CreatedAt <= filter.ToDate
                            && (string.IsNullOrEmpty(searchString)
                                || (isSearchYou && (a.PerformedById == currentUserId || a.PerformedOnId == currentUserId))
                                || a.PerformedOnUser.FirstName.ToLower().Contains(searchString)
                                || a.PerformedOnUser.LastName.ToLower().Contains(searchString)
                                || a.Group.Name.ToLower().Contains(searchString)
                                || a.Expense.Title.ToLower().Contains(searchString)
                                || a.Expense.Amount.ToString().Contains(searchString)
                                || a.Payment.Amount.ToString().Contains(searchString)
                            ),
            orderBy: a => a.OrderByDescending(a => a.CreatedAt),
            includes: new List<System.Linq.Expressions.Expression<Func<Activity, object>>>
            {
                a => a.Group,
                a => a.PerformedByUser,
                a => a.PerformedOnUser,
                a => a.Expense,
                a => a.Payment
            }
        );

        return userActivities.Items.ToList();

    }

    public async Task<ActivityFilterVM> GetActivityFilter(int? groupId = null)
    {
        int currentUserId = _userService.LoggedInUserId();
        ActivityFilterVM activityFilter = new ActivityFilterVM();
        activityFilter.FirstDay = DateHelper.FirstDayOfMonth(DateTime.Now);
        activityFilter.LastDay = DateHelper.LastDayOfMonth(DateTime.Now);

        // First activity
        Activity firstActivity = await _activityRepository.FirstOrLast(
            isFirstElement: true,
            predicate: a => a.DeletedAt == null
                        && (groupId != null ? a.GroupId == groupId : (a.PerformedById == currentUserId || a.PerformedOnId == currentUserId)),
            orderBy: a => a.OrderBy(a => a.CreatedAt)
        );

        // Last activity
        Activity lastActivity = await _activityRepository.FirstOrLast(
            isFirstElement: false,
            predicate: a => a.DeletedAt == null
                        && (groupId != null ? a.GroupId == groupId : (a.PerformedById == currentUserId || a.PerformedOnId == currentUserId)),
            orderBy: a => a.OrderBy(a => a.CreatedAt)
        );

        activityFilter.MinDate = firstActivity.CreatedAt;
        activityFilter.MaxDate = lastActivity.CreatedAt;
        return activityFilter;
    }
}
