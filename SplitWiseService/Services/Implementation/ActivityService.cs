using Microsoft.EntityFrameworkCore;
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

    public async Task AddActivity(ActivityType activityType, List<int> userIds, int? groupId = null, int? expenseId = null, int? paymentId = null, int? performedOnId = null, string additionalDetails = null, string groupName = null, string amount = null, string groupImagePath = null)
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
            AdditionalDetails = additionalDetails,
            GroupName = groupName,
            GroupImagePath = groupImagePath,
            Amount = amount,
            UserIds = string.Join(",", userIds),
            CreatedById = currentUserId,
            UpdatedAt = DateTime.Now,
            UpdatedById = currentUserId
        };

        await _activityRepository.Add(activity);
        return;
    }

    public async Task<List<Activity>> ActivityList(FilterVM filter, int? groupId = null, int? friendUserId = null)
    {
        int currentUserId = _userService.LoggedInUserId();
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? string.Empty : filter.SearchString.Trim().ToLower();

        bool isSearchTextYou = "you".Contains(searchString);
        DateTime newToDate = ((DateTime)filter.ToDate).AddDays(1);

        PaginatedItemsVM<Activity> userActivities = await _activityRepository.PaginatedList(
            predicate: a => a.DeletedAt == null
                            && (groupId > 0
                                ? a.GroupId == groupId
                                : (friendUserId == null
                                    ? a.PerformedById == currentUserId || a.PerformedOnId == currentUserId || a.UserIds.Contains(currentUserId.ToString())
                                    : ((a.PerformedById == currentUserId && (a.PerformedOnId == friendUserId || a.UserIds.Contains(friendUserId.ToString())))
                                        || (a.PerformedById == friendUserId && (a.PerformedOnId == currentUserId || a.UserIds.Contains(currentUserId.ToString()))))))
                            && a.CreatedAt >= filter.FromDate
                            && a.CreatedAt < newToDate
                            && (string.IsNullOrEmpty(searchString)
                                || (isSearchTextYou && (a.PerformedById == currentUserId || a.PerformedOnId == currentUserId))
                                || a.PerformedByUser.FirstName.ToLower().Contains(searchString)
                                || a.PerformedByUser.LastName.ToLower().Contains(searchString)
                                || (a.PerformedByUser.FirstName + " " + a.PerformedByUser.LastName).ToLower().Contains(searchString)
                                || a.PerformedOnUser.FirstName.ToLower().Contains(searchString)
                                || a.PerformedOnUser.LastName.ToLower().Contains(searchString)
                                || (a.PerformedOnUser.FirstName + " " + a.PerformedOnUser.LastName).ToLower().Contains(searchString)
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
            },
            thenIncludes: new List<Func<IQueryable<Activity>, IQueryable<Activity>>>
            {
                a => a.Include(a => a.Group)
                    .ThenInclude(g => g.GroupMembers)
            }
        );

        return userActivities.Items.ToList();
    }

    public async Task<ActivityFilterVM> GetActivityFilter(int? groupId = null, int? friendUserId = null)
    {
        ActivityFilterVM activityFilter = new ActivityFilterVM();
        activityFilter.FirstDay = DateHelper.FirstDayOfMonth(DateTime.Now);
        activityFilter.LastDay = DateHelper.LastDayOfMonth(DateTime.Now);

        return activityFilter;
    }

    public async Task<byte[]> ExportActivity(FilterVM filter, int? groupId = null, int? friendUserId = null)
    {
        filter.PageNumber = 0;
        filter.PageSize = 0;
        List<Activity> activitieList = await ActivityList(filter, groupId: groupId, friendUserId: friendUserId);

        List<ActivityVM> activities = activitieList.Select(a =>
        {
            // Usernames
            string performedByUserName = a.PerformedByUser.FirstName + " " + a.PerformedByUser.LastName;
            string performedOnUserName = string.Empty;
            if (a.PerformedOnUser != null)
            {
                performedOnUserName = a.PerformedOnUser.FirstName + " " + a.PerformedOnUser.LastName;
            }

            // Group name
            string groupName = string.Empty;
            if (a.Group != null)
            {
                groupName = a.GroupName ?? a.Group.Name;
            }

            // Expense name, amount
            string expenseName = string.Empty;
            string expenseAmount = string.Empty;
            if (a.Expense != null)
            {
                expenseName = a.Expense.Title;
                expenseAmount = "₹" + (a.Amount ?? a.Expense.Amount.ToString("N2"));
            }

            // Payment
            string paymentAmount = string.Empty;
            if (a.Payment != null)
            {
                paymentAmount = "₹" + a.Payment.Amount.ToString("N2");
            }

            string message = performedByUserName;
            switch (a.ActivityType)
            {
                case ActivityType.GroupCreated:
                    message += $" created group {groupName}.";
                    break;
                case ActivityType.GroupUpdated:
                    message += $" updated group {groupName}.";
                    break;
                case ActivityType.GroupDeleted:
                    message += $" deleted group {groupName}.";
                    break;
                case ActivityType.MemberAdded:
                    message += $" added {performedOnUserName} to the group {groupName}.";
                    break;
                case ActivityType.MemberRemoved:
                    message += $" removed {performedOnUserName} from the group {groupName}.";
                    break;
                case ActivityType.LeaveGroup:
                    message += $" left the group {groupName}.";
                    break;
                case ActivityType.GroupExpenseAdded:
                    message += $" added an expense {expenseName} of {expenseAmount} in the group {groupName}.";
                    break;
                case ActivityType.GroupExpenseUpdated:
                    message += $" updated an expense {expenseName} in the group {groupName}.";
                    break;
                case ActivityType.GroupPaymenent:
                    message += $" paid {paymentAmount} to {performedOnUserName} in the group {groupName}.";
                    break;
                case ActivityType.NonGroupPaymenent:
                    message += $" paid {paymentAmount} to {performedOnUserName}.";
                    break;
                case ActivityType.ExpenseAdded:
                    message += $" added an expense {expenseName} of {expenseAmount}.";
                    break;
                case ActivityType.ExpenseUpdated:
                    message += $" updated an expense {expenseName}.";
                    break;
                case ActivityType.GroupExpenseDeleted:
                    message += $" deleted an expense {expenseName} in the group {groupName}.";
                    break;
                case ActivityType.NonGroupExpenseDeleted:
                    message += $" deleted an expense {expenseName}.";
                    break;
            }

            return new ActivityVM()
            {
                Date = a.CreatedAt.ToString("dd-MM-yyyy"),
                Time = a.CreatedAt.ToString("HH:mm:ss"),
                ActivityMessage = message + (string.IsNullOrEmpty(a.AdditionalDetails)
                                            ? string.Empty
                                            : $" ({a.AdditionalDetails.Replace("<strong>", string.Empty).Replace("</strong>", string.Empty)})")
            };
        }).ToList();

        if (!activities.Any())
        {
            return null;
        }
        return ExcelExportHelper.ExportToExcel(activities, filter, "Activities");
    }

}
