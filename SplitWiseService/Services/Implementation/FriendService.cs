using System.Linq.Expressions;
using MailKit.Search;
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
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public FriendService(IUserService userService, IEmailService emailService, IGenericRepository<FriendRequest> friendRequestRepository, ITransactionRepository transaction, IGenericRepository<UserReferral> userReferralRepository)
    {
        _userService = userService;
        _emailService = emailService;
        _friendRequestRepository = friendRequestRepository;
        _transaction = transaction;
        _userReferralRepository = userReferralRepository;
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

        filter.SearchQuery = string.IsNullOrEmpty(filter.SearchQuery) ? "" : filter.SearchQuery.Replace(" ", "").ToLower();

        Func<IQueryable<FriendRequest>, IOrderedQueryable<FriendRequest>> orderBy = q => q.OrderBy(fr => fr.Id);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn)
            {
                case "name":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(fr => fr.ReceiverUserNavigation.FirstName) : q => q.OrderByDescending(fr => fr.ReceiverUserNavigation.FirstName);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<FriendRequest> paginatedItems = await _friendRequestRepository.PaginatedList(
            predicate: fr => fr.ReceiverId == userId && fr.Status == FeriendRequestStatus.Requested && (string.IsNullOrEmpty(filter.SearchQuery) ||fr.ReceiverUserNavigation.FirstName.ToLower().Contains(filter.SearchQuery) || fr.ReceiverUserNavigation.LastName.ToLower().Contains(filter.SearchQuery)),
            orderBy: orderBy,
            includes: new List<Expression<Func<FriendRequest, object>>>
            {
                fr => fr.ReceiverUserNavigation
            },
            pageSize: filter.PageSize,
            pageNumber: filter.PageNumber
        );

        FriendRequestListVM friendRequests = new FriendRequestListVM();

        friendRequests.friendRequestList = paginatedItems.Items.Select(fr => new FriendRequestVM
        {
            Id = fr.ReceiverUserNavigation.Id,
            Name = $"{fr.ReceiverUserNavigation.FirstName} {fr.ReceiverUserNavigation.LastName}",
            Email = fr.ReceiverUserNavigation.EmailAddress,
            ProfileImagePath = fr.ReceiverUserNavigation.ProfileImagePath
        }).ToList();

        friendRequests.Page.SetPagination(paginatedItems.totalRecords, filter.PageSize, filter.PageNumber);

        return friendRequests;
    }
}
