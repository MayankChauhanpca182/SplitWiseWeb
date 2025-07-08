using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class ActivityService : IActivityService
{
    // private readonly IGenericRepository<UserActivity> _userActivityService;
    private readonly IGenericRepository<GroupActivity> _groupActivityRepository;
    private readonly IUserService _userService;

    public ActivityService(IGenericRepository<GroupActivity> groupActivityRepository, IUserService userService)
    {
        _groupActivityRepository = groupActivityRepository;
        _userService = userService;
    }

    public async Task AddGroupActivity(ActivityType activityType, int groupId, int? expenseId = null, int? paymentId = null, int? performedOnId = null)
    {
        int currentUserId = _userService.LoggedInUserId();

        GroupActivity groupActivity = new GroupActivity
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

        await _groupActivityRepository.Add(groupActivity);
        return;
    }

    public async Task<List<ActivityVM>> GroupActivityList(int groupId)
    {
        int currentUserId = _userService.LoggedInUserId();

        List<GroupActivity> groupActivities = await _groupActivityRepository.List(
            predicate: ge => ge.DeletedAt == null && ge.GroupId == groupId,
            orderBy: ge => ge.OrderByDescending(ge => ge.CreatedAt),
            includes: new List<System.Linq.Expressions.Expression<Func<GroupActivity, object>>>
            {
                ge => ge.Group,
                ge => ge. PerformedByUser,
                ge => ge. PerformedOnUser,
                ge => ge.Expense,
                ge => ge.Payment
            }
        );

        List<ActivityVM> activityList = groupActivities.Select(ge =>
        {
            string performedByUserName = ge.PerformedByUser.Id == currentUserId ? "You" : $"{ge.PerformedByUser.FirstName} {ge.PerformedByUser.LastName}";

            string performedOnUserName = string.Empty;
            if (ge.PerformedOnUser != null)
            {
                performedOnUserName = ge.PerformedOnUser.Id == currentUserId ? "You" : $"{ge.PerformedOnUser.FirstName} {ge.PerformedOnUser.LastName}";
            }

            string activityMessage = string.Empty;
            switch (ge.ActivityType)
            {
                case ActivityType.GroupCreated:
                    activityMessage = ActivityMessages.GroupCreated
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", ge.Group.Name);
                    break;
                case ActivityType.GroupUpdated:
                    activityMessage = ActivityMessages.GroupUpdated
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", ge.Group.Name);
                    break;
                case ActivityType.GroupDeleted:
                    activityMessage = ActivityMessages.GroupDeleted
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", ge.Group.Name);
                    break;
                case ActivityType.MemberAdded:
                    activityMessage = ActivityMessages.MemberAdded
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", performedOnUserName)
                                        .Replace("{2}", ge.Group.Name);
                    break;
                case ActivityType.MemberRemoved:
                    activityMessage = ActivityMessages.MemberRemoved
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", performedOnUserName)
                                        .Replace("{2}", ge.Group.Name);
                    break;
                case ActivityType.GroupExpenseAdded:
                    activityMessage = ActivityMessages.GroupExpenseAdded
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", ge.Expense.Title)
                                        .Replace("{2}", ge.Group.Name);
                    break;
                case ActivityType.GroupExpenseUpdated:
                    activityMessage = ActivityMessages.GroupExpenseUpdated
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", ge.Expense.Title)
                                        .Replace("{2}", ge.Group.Name);
                    break;
                case ActivityType.Paid:
                    activityMessage = ActivityMessages.Paid
                                        .Replace("{0}", performedByUserName)
                                        .Replace("{1}", ge.Payment.Amount.ToString("N2"))
                                        .Replace("{2}", performedOnUserName);
                    break;
            }

            return new ActivityVM
            {
                ImagePath = ge.PerformedByUser.ProfileImagePath,
                ActivityMessage = activityMessage,
                CreatedAt = ge.CreatedAt
            };
        }).ToList();
        
        return activityList;
    }

}
