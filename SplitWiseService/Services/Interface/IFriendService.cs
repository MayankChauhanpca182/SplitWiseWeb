using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IFriendService
{
    public Task<Friend> GetFriend(int user1Id, int user2Id);
    public Task<ResponseVM> CheckExisitngFrindship(string email);
    public Task<ResponseVM> SendRequest(string emailAddress);
    public Task<ResponseVM> SendReferral(FriendRequestVM request);
    public Task<PaginatedListVM<FriendRequestVM>> FriendRequestList(FilterVM filter);
    public Task<ResponseVM> AcceptRequest(int requestId);
    public Task<ResponseVM> RejectRequest(int requestId);
    public Task<PaginatedListVM<FriendVM>> FriendList(FilterVM filter, int groupId = 0);
    public Task<ResponseVM> RemoveFriend(int friendId);
    public Task UpdateReferrals(User newUser);
    public Task<byte[]> ExportFriends(FilterVM filter);
}
