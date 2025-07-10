using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
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

}
