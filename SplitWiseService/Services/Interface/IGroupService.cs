using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IGroupService
{
    public Task<GroupVM> GetGroup(int groupId);
    public Task<ResponseVM> SaveGroup(GroupVM newGroup);
    public Task<PaginatedListVM<GroupVM>> GroupList(FilterVM filter);
    public Task<ResponseVM> DeleteGroup(int groupId);
    public Task<List<GroupMemberVM>> GetMembers(int groupId);
    public Task<ResponseVM> AddGroupMembers(int groupId, int userId);
    public Task<ResponseVM> RemoveGroupMembers(int groupMemberId);
}
