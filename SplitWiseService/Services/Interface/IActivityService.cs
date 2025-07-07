using SplitWiseRepository.Constants;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IActivityService
{
    public Task AddGroupActivity(ActivityType activityType, int groupId, int? expenseId = null, int? paymentId = null, int? performedOnId = null);
    public Task<List<ActivityVM>> GroupActivityList(int groupId);
}
