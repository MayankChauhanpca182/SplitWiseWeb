using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IFriendService
{
    public Task<ResponseVM> SendRequest(FriendRequestVM requests);
    public Task<ResponseVM> SendReferral(FriendRequestVM request);
    public Task<FriendRequestListVM> FriendRequestList(FilterVM filter);
    public Task<ResponseVM> AcceptRequest(int requestId);
    public Task<ResponseVM> RejectRequest(int requestId);
    public Task<FriendListVM> FriendList(FilterVM filter);
}
