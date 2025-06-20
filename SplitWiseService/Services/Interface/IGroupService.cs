using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IGroupService
{
    public Task<GroupVM> GetGroup(int groupId);
    public Task<ResponseVM> SaveGroup(GroupVM newGroup);
    public Task<PaginatedListVM<GroupVM>> GroupList(FilterVM filter);
}
