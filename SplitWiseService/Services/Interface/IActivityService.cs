using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IActivityService
{
    public Task AddActivity(ActivityType activityType, int? groupId = null, int? expenseId = null, int? paymentId = null, int? performedOnId = null);
    public Task<List<Activity>> GroupActivityList(int groupId);
    public Task<List<Activity>> UserActivityList();
    public Task<List<Activity>> ActivityList(FilterVM filter, int? groupId = null);
}
