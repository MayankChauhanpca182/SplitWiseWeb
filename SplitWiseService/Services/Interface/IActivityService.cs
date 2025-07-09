using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IActivityService
{
    public Task AddGroupActivity(ActivityType activityType, int? groupId = null, int? expenseId = null, int? paymentId = null, int? performedOnId = null);
    public Task<List<GroupActivity>> GroupActivityList(int groupId);
}
