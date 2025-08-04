using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IActivityService
{
    public Task AddActivity(ActivityType activityType, List<int> userIds, int? groupId = null, int? expenseId = null, int? paymentId = null, int? performedOnId = null, string additionalDetails = null, string amount = null);
    public Task<List<Activity>> ActivityList(FilterVM filter, int? groupId = null, int? friendUserId = null);
    public ActivityFilterVM GetActivityFilter(int? groupId = null, int? friendUserId = null);
    public Task<byte[]> ExportActivity(FilterVM filter, int? groupId = null, int? friendUserId = null);
}
