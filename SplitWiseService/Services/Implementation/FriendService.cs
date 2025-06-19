using System.Linq.Expressions;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class FriendService : IFriendService
{
    private readonly IGenericRepository<FriendRequest> _friendRequestRepository;
    private readonly ITransactionRepository _transaction;
    private readonly IGenericRepository<UserReferral> _userReferralRepository;
    private readonly IGenericRepository<Friend> _friendRepository;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public FriendService(IUserService userService, IEmailService emailService, IGenericRepository<FriendRequest> friendRequestRepository, ITransactionRepository transaction, IGenericRepository<UserReferral> userReferralRepository, IGenericRepository<Friend> friendRepository)
    {
        _userService = userService;
        _emailService = emailService;
        _friendRequestRepository = friendRequestRepository;
        _transaction = transaction;
        _userReferralRepository = userReferralRepository;
        _friendRepository = friendRepository;
    }

    private async Task AddFriendRequest(int requesterId, int? receiverId = null, int? referralId = null)
    {
        User currentUser = await _userService.LoggedInUser();
        FriendRequest newFriendRequest = new FriendRequest
        {
            RequesterId = requesterId,
            ReceiverId = receiverId,
            ReferralId = referralId,
            Status = FeriendRequestStatus.Requested,
            CreatedById = currentUser.Id,
            UpdatedAt = DateTime.Now,
            UpdatedById = currentUser.Id
        };
        await _friendRequestRepository.Add(newFriendRequest);
    }

    public async Task<ResponseVM> CheckExisitngFrindship(string email)
    {
        ResponseVM response = new ResponseVM();

        User currentUser = await _userService.LoggedInUser();
        if (email == currentUser.EmailAddress)
        {
            response.Success = false;
            response.Message = NotificationMessages.FriendRequestToSelf;
            return response;
        }
        User requestedUser = await _userService.GetByEmailAddress(email);
        bool isAlreadyFriend = await _friendRepository.Any(f => f.DeletedAt == null && ((f.Friend1 == currentUser.Id && f.Friend2 == requestedUser.Id) || (f.Friend2 == currentUser.Id && f.Friend1 == requestedUser.Id)));
        if (isAlreadyFriend)
        {
            response.Success = false;
            response.Message = NotificationMessages.AlreadyFriend.Replace("{0}", $"{requestedUser.FirstName} {requestedUser.LastName}");
        }
        else
        {
            response.Success = true;
        }
        return response;
    }

    public async Task<ResponseVM> SendRequest(FriendRequestVM requests)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            ResponseVM response = new ResponseVM();
            User existingUser = await _userService.GetByEmailAddress(requests.Email);
            if (existingUser == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NoAccountFound.Replace("{0}", requests.Email);
            }
            else
            {
                User currentUser = await _userService.LoggedInUser();
                await AddFriendRequest(currentUser.Id, existingUser.Id);

                // Send email
                await _emailService.FriendRequestEmail(existingUser.FirstName, currentUser.FirstName, existingUser.EmailAddress);

                response.Success = true;
                response.Message = NotificationMessages.FriendRequestSuccess;
            }

            // Commit transaction
            await _transaction.Commit();
            return response;
        }
        catch
        {
            // Rollback transaction
            await _transaction.Rollback();
            throw;
        }
    }

    public async Task<ResponseVM> SendReferral(FriendRequestVM request)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            ResponseVM response = new ResponseVM();
            response.Message = NotificationMessages.ReferralSuccess;

            User currentUser = await _userService.LoggedInUser();

            // Check if user has already referred
            if (await _userReferralRepository.Any(ur => ur.ReferredToEmailAddress.ToLower() == request.Email.ToLower() && !ur.IsAccountRegistered && ur.ReferredFromUserId == currentUser.Id))
            {
                response.Success = false;
                response.Message = NotificationMessages.AlreadyReferred.Replace("{0}", request.Email);
                return response;
            }

            // Check if already referred
            if (await _userReferralRepository.Any(ur => ur.ReferredToEmailAddress.ToLower() == request.Email.ToLower() && !ur.IsAccountRegistered))
            {
                response.Message = NotificationMessages.AlreadyReferredBySomeone.Replace("{0}", request.Email);
            }

            // Add into user referral
            UserReferral userReferral = new UserReferral
            {
                ReferredFromUserId = currentUser.Id,
                ReferredFromEmailAddress = currentUser.EmailAddress,
                ReferredToEmailAddress = request.Email,
            };
            await _userReferralRepository.Add(userReferral);

            // Add friend request
            await AddFriendRequest(currentUser.Id, referralId: userReferral.Id);

            // Send email
            await _emailService.ReferralEmail($"{currentUser.FirstName} {currentUser.LastName}", request.Email);

            response.Success = true;

            // Commit transaction
            await _transaction.Commit();
            return response;
        }
        catch
        {
            // Rollback transaction
            await _transaction.Rollback();
            throw;
        }
    }

    public async Task<FriendRequestListVM> FriendRequestList(FilterVM filter)
    {
        int userId = _userService.LoggedInUserId();

        filter.SearchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(@"\s+", "").ToLower();

        Func<IQueryable<FriendRequest>, IOrderedQueryable<FriendRequest>> orderBy = q => q.OrderBy(fr => fr.Id);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn)
            {
                case "name":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(fr => fr.ReceiverUserNavigation.FirstName) : q => q.OrderByDescending(fr => fr.ReceiverUserNavigation.FirstName);
                    break;
                case "email":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(fr => fr.ReceiverUserNavigation.EmailAddress) : q => q.OrderByDescending(fr => fr.ReceiverUserNavigation.EmailAddress);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<FriendRequest> paginatedItems = await _friendRequestRepository.PaginatedList(
            predicate: fr => fr.ReceiverId == userId
            && fr.Status == FeriendRequestStatus.Requested
            && (string.IsNullOrEmpty(filter.SearchString)
                || fr.ReceiverUserNavigation.FirstName.ToLower().Contains(filter.SearchString)
                || fr.ReceiverUserNavigation.LastName.ToLower().Contains(filter.SearchString)
                || fr.ReceiverUserNavigation.EmailAddress.ToLower().Contains(filter.SearchString)),
            orderBy: orderBy,
            includes: new List<Expression<Func<FriendRequest, object>>>
            {
                fr => fr.RequesterUserNavigation
            },
            pageSize: filter.PageSize,
            pageNumber: filter.PageNumber
        );

        FriendRequestListVM friendRequests = new FriendRequestListVM();

        friendRequests.FriendRequestList = paginatedItems.Items.Select(fr => new FriendRequestVM
        {
            Id = fr.Id,
            Name = $"{fr.RequesterUserNavigation.FirstName} {fr.RequesterUserNavigation.LastName}",
            Email = fr.RequesterUserNavigation.EmailAddress,
            ProfileImagePath = fr.RequesterUserNavigation.ProfileImagePath
        }).ToList();

        friendRequests.Page.SetPagination(paginatedItems.totalRecords, filter.PageSize, filter.PageNumber);

        return friendRequests;
    }

    public async Task<ResponseVM> AcceptRequest(int requestId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            int userId = _userService.LoggedInUserId();
            ResponseVM response = new ResponseVM();

            FriendRequest friendRequest = await _friendRequestRepository.Get(fr => fr.Id == requestId);
            if (friendRequest == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NotFound.Replace("{0}", "friend request");
                return response;
            }
            else
            {
                friendRequest.Status = FeriendRequestStatus.Accepted;
                friendRequest.UpdatedAt = DateTime.Now;
                friendRequest.UpdatedById = userId;
                await _friendRequestRepository.Update(friendRequest);

                // Add into friends table
                Friend friend = new Friend
                {
                    Friend1 = friendRequest.RequesterId,
                    Friend2 = (int)friendRequest.ReceiverId,
                    CreatedById = userId,
                    UpdatedAt = DateTime.Now,
                    UpdatedById = userId
                };
                await _friendRepository.Add(friend);

                User requesterUser = await _userService.GetById(friendRequest.RequesterId);
                User receiverUser = await _userService.GetById((int)friendRequest.ReceiverId);

                // Send email
                await _emailService.FriendRequestAcceptedEmail(requesterUser.FirstName, $"{receiverUser.FirstName} {receiverUser.LastName}", requesterUser.EmailAddress);

                response.Success = true;
                response.Message = NotificationMessages.FriendRequestAccepted.Replace("{0}", $"{requesterUser.FirstName} {requesterUser.LastName}");
            }

            // Commit transaction
            await _transaction.Commit();
            return response;
        }
        catch
        {
            // Rollback transaction
            await _transaction.Rollback();
            throw;
        }
    }

    public async Task<ResponseVM> RejectRequest(int requestId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            int userId = _userService.LoggedInUserId();
            ResponseVM response = new ResponseVM();

            FriendRequest friendRequest = await _friendRequestRepository.Get(fr => fr.Id == requestId);
            if (friendRequest == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NotFound.Replace("{0}", "friend request");
                return response;
            }
            else
            {
                friendRequest.Status = FeriendRequestStatus.Rejected;
                friendRequest.UpdatedAt = DateTime.Now;
                friendRequest.UpdatedById = userId;
                await _friendRequestRepository.Update(friendRequest);

                User requesterUser = await _userService.GetById(friendRequest.RequesterId);
                User receiverUser = await _userService.GetById((int)friendRequest.ReceiverId);

                // Send email
                await _emailService.FriendRequestRejectedEmail(requesterUser.FirstName, $"{receiverUser.FirstName} {receiverUser.LastName}", requesterUser.EmailAddress);

                response.Success = true;
                response.Message = NotificationMessages.FriendRequestRejected.Replace("{0}", $"{requesterUser.FirstName} {requesterUser.LastName}");
            }

            // Commit transaction
            await _transaction.Commit();
            return response;
        }
        catch
        {
            // Rollback transaction
            await _transaction.Rollback();
            throw;
        }
    }

    public async Task<FriendListVM> FriendList(FilterVM filter)
    {
        int userId = _userService.LoggedInUserId();
        filter.SearchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(@"\s+", "").ToLower();

        Func<IQueryable<Friend>, IOrderedQueryable<Friend>> orderBy = q => q.OrderBy(f => f.Id);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn.ToLower())
            {
                case "name":
                    orderBy = filter.SortOrder == "asc"
                        ? q => q.OrderBy(f => f.Friend1 == userId ? f.Friend2UserNavigation.FirstName : f.Friend1UserNavigation.FirstName)
                        : q => q.OrderByDescending(f => f.Friend1 == userId ? f.Friend2UserNavigation.FirstName : f.Friend1UserNavigation.FirstName);
                    break;
                case "email":
                    orderBy = filter.SortOrder == "asc"
                        ? q => q.OrderBy(f => f.Friend1 == userId ? f.Friend2UserNavigation.EmailAddress : f.Friend1UserNavigation.EmailAddress)
                        : q => q.OrderByDescending(f => f.Friend1 == userId ? f.Friend2UserNavigation.EmailAddress : f.Friend1UserNavigation.EmailAddress);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<Friend> paginatedItems = await _friendRepository.PaginatedList(
            predicate: f => (f.Friend1 == userId || f.Friend2 == userId)
                && f.DeletedAt == null
                && (string.IsNullOrEmpty(filter.SearchString)
                    || (f.Friend1 == userId
                        ? (f.Friend2UserNavigation.FirstName.ToLower().Contains(filter.SearchString)
                            || f.Friend2UserNavigation.LastName.ToLower().Contains(filter.SearchString)
                            || f.Friend2UserNavigation.EmailAddress.ToLower().Contains(filter.SearchString))
                        : (f.Friend1UserNavigation.FirstName.ToLower().Contains(filter.SearchString)
                            || f.Friend1UserNavigation.LastName.ToLower().Contains(filter.SearchString)
                            || f.Friend1UserNavigation.EmailAddress.ToLower().Contains(filter.SearchString)))),
            orderBy: orderBy,
            includes: new List<Expression<Func<Friend, object>>>
            {
                fr => fr.Friend1UserNavigation,
                fr => fr.Friend2UserNavigation
            },
            pageSize: filter.PageSize,
            pageNumber: filter.PageNumber
        );

        FriendListVM friendList = new FriendListVM();
        friendList.FriendList = paginatedItems.Items.Select(f => new FriendVM
        {
            FriendId = f.Id,
            UserId = f.Friend2 == userId ? f.Friend1UserNavigation.Id : f.Friend2UserNavigation.Id,
            Name = f.Friend2 == userId ? f.Friend1UserNavigation.FirstName + " " + f.Friend1UserNavigation.LastName : f.Friend2UserNavigation.FirstName + " " + f.Friend2UserNavigation.LastName,
            EmailAddress = f.Friend2 == userId ? f.Friend1UserNavigation.EmailAddress : f.Friend2UserNavigation.EmailAddress,
            ProfileImagePath = f.Friend2 == userId ? f.Friend1UserNavigation.ProfileImagePath : f.Friend2UserNavigation.ProfileImagePath,
        }).ToList();

        friendList.Page.SetPagination(paginatedItems.totalRecords, filter.PageSize, filter.PageNumber);

        return friendList;
    }

    public async Task<ResponseVM> RemoveFriend(int friendId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            int userId = _userService.LoggedInUserId();
            ResponseVM response = new ResponseVM();

            Friend friend = await _friendRepository.Get(f => f.Id == friendId);

            friend.DeletedAt = DateTime.Now;
            friend.DeletedById = userId;
            await _friendRepository.Update(friend);

            int friendUserId = friend.Friend1 == userId ? friend.Friend2 : friend.Friend1;
            User friendUser = await _userService.GetById(friendUserId);

            response.Success = true;
            response.Message = NotificationMessages.FriendRemoved.Replace("{0}", friendUser.FirstName);

            // Commit transaction
            await _transaction.Commit();
            return response;
        }
        catch
        {
            // Rollback transaction
            await _transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateReferrals(User newUser)
    {
        List<UserReferral> referralList = await _userReferralRepository.List(ur => ur.ReferredToEmailAddress.ToLower() == newUser.EmailAddress.ToLower() && !ur.IsAccountRegistered);

        foreach (UserReferral referral in referralList)
        {
            referral.IsAccountRegistered = true;
            referral.RegisteredAt = DateTime.Now;
            await _userReferralRepository.Update(referral);

            // Add user id to friend request
            await AddUserIdToFriendRequest(referral.Id, newUser.Id);
        }
        return;
    }

    private async Task AddUserIdToFriendRequest(int referralId, int newUserId)
    {
        FriendRequest friendRequest = await _friendRequestRepository.Get(fr => fr.ReferralId == referralId);
        friendRequest.ReceiverId = newUserId;
        await _friendRequestRepository.Update(friendRequest);
        return;
    }
}
