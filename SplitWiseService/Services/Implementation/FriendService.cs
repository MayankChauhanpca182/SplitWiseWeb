using System.Linq.Expressions;
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
    private readonly IGenericRepository<Expense> _expenseRepository;
    private readonly IGenericRepository<ExpenseShare> _expenseShareRepository;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public FriendService(IUserService userService, IEmailService emailService, IGenericRepository<FriendRequest> friendRequestRepository, ITransactionRepository transaction, IGenericRepository<UserReferral> userReferralRepository, IGenericRepository<Friend> friendRepository, IGenericRepository<Expense> expenseRepository, IGenericRepository<ExpenseShare> expenseShareRepository)
    {
        _userService = userService;
        _emailService = emailService;
        _friendRequestRepository = friendRequestRepository;
        _transaction = transaction;
        _userReferralRepository = userReferralRepository;
        _friendRepository = friendRepository;
        _expenseRepository = expenseRepository;
        _expenseShareRepository = expenseShareRepository;
    }

    private async Task AddFriendRequest(int requesterId, int? receiverId = null, int? referralId = null)
    {
        User currentUser = await _userService.LoggedInUser();
        FriendRequest existingFriendRequest = await _friendRequestRepository.Get(fr => fr.Status != FeriendRequestStatus.Requested && ((fr.RequesterId == requesterId && fr.ReceiverId == receiverId) || (fr.RequesterId == receiverId && fr.ReceiverId == requesterId)));
        if (existingFriendRequest != null)
        {
            // Update exisiting one
            existingFriendRequest.RequesterId = requesterId;
            existingFriendRequest.ReceiverId = receiverId;
            existingFriendRequest.Status = FeriendRequestStatus.Requested;
            existingFriendRequest.UpdatedAt = DateTime.Now;
            existingFriendRequest.UpdatedById = currentUser.Id;
            await _friendRequestRepository.Update(existingFriendRequest);
        }
        else
        {
            // Add new friend request
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
    }

    public async Task<ResponseVM> CheckExisitngFrindship(string email)
    {
        ResponseVM response = new ResponseVM();

        User currentUser = await _userService.LoggedInUser();
        if (email.ToLower() == currentUser.EmailAddress)
        {
            response.Success = false;
            response.Message = NotificationMessages.FriendRequestToSelf;
            return response;
        }
        User requestedUser = await _userService.GetByEmailAddress(email);
        if (requestedUser != null)
        {
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
        else
        {
            response.Success = true;
        }

        return response;
    }

    public async Task<ResponseVM> SendRequest(string emailAddress)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            ResponseVM response = new ResponseVM();
            User requestedUser = await _userService.GetByEmailAddress(emailAddress);

            if (requestedUser == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NoAccountFound.Replace("{0}", emailAddress);
                response.ShowNextAction = true;
            }
            else
            {
                User currentUser = await _userService.LoggedInUser();

                string requesterFullName = $"{requestedUser.FirstName} {requestedUser.LastName}";
                string currentUserFullName = $"{currentUser.FirstName} {currentUser.LastName}";

                // Check if already requested
                if (await _friendRequestRepository.Any(fr => fr.Status == FeriendRequestStatus.Requested && fr.ReceiverId == requestedUser.Id && fr.RequesterId == currentUser.Id))
                {
                    response.Success = false;
                    response.Message = NotificationMessages.YouAlreadyRequested.Replace("{0}", requesterFullName);
                    return response;
                }
                // Check if requeste already exist
                else if (await _friendRequestRepository.Any(fr => fr.Status == FeriendRequestStatus.Requested && fr.ReceiverId == currentUser.Id && fr.RequesterId == requestedUser.Id))
                {
                    response.Success = false;
                    response.Message = NotificationMessages.FriendRequestExist.Replace("{0}", requesterFullName).Replace("{1}", requestedUser.EmailAddress);
                    return response;
                }

                await AddFriendRequest(currentUser.Id, requestedUser.Id);

                // Send email
                await _emailService.FriendRequestEmail(requestedUser.FirstName, currentUserFullName, requestedUser.EmailAddress);

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

            User currentUser = await _userService.LoggedInUser();

            // Check if user has already referred
            if (await _userReferralRepository.Any(ur => ur.ReferredToEmailAddress.ToLower() == request.Email.ToLower() && !ur.IsAccountRegistered && ur.ReferredFromUserId == currentUser.Id))
            {
                response.Success = false;
                response.Message = NotificationMessages.AlreadyReferred.Replace("{0}", request.Email);
                return response;
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
            response.Message = NotificationMessages.ReferralSuccess;

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

    public async Task<PaginatedListVM<FriendRequestVM>> FriendRequestList(FilterVM filter)
    {
        int currentUserId = _userService.LoggedInUserId();

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
            predicate: fr => fr.ReceiverId == currentUserId
            && fr.Status == FeriendRequestStatus.Requested
            && (string.IsNullOrEmpty(filter.SearchString)
                ||(fr.RequesterId == currentUserId
                    ? (fr.ReceiverUserNavigation.FirstName.ToLower().Contains(filter.SearchString)
                        || fr.ReceiverUserNavigation.LastName.ToLower().Contains(filter.SearchString)
                        || fr.ReceiverUserNavigation.EmailAddress.ToLower().Contains(filter.SearchString))
                    : (fr.RequesterUserNavigation.FirstName.ToLower().Contains(filter.SearchString)
                        || fr.RequesterUserNavigation.LastName.ToLower().Contains(filter.SearchString)
                        || fr.RequesterUserNavigation.EmailAddress.ToLower().Contains(filter.SearchString)))),
            orderBy: orderBy,
            includes: new List<Expression<Func<FriendRequest, object>>>
            {
                fr => fr.RequesterUserNavigation
            },
            pageSize: filter.PageSize,
            pageNumber: filter.PageNumber
        );

        PaginatedListVM<FriendRequestVM> paginatedList = new PaginatedListVM<FriendRequestVM>();

        paginatedList.List = paginatedItems.Items.Select(fr => new FriendRequestVM
        {
            Id = fr.Id,
            Name = $"{fr.RequesterUserNavigation.FirstName} {fr.RequesterUserNavigation.LastName}",
            Email = fr.RequesterUserNavigation.EmailAddress,
            ProfileImagePath = fr.RequesterUserNavigation.ProfileImagePath
        }).ToList();

        paginatedList.Page.SetPagination(paginatedItems.TotalRecords, filter.PageSize, filter.PageNumber);

        return paginatedList;
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

                // Fetch existing friend
                Friend exisitngFriend = await _friendRepository.Get(f => (f.Friend1 == friendRequest.RequesterId && f.Friend2 == friendRequest.ReceiverId) || (f.Friend2 == friendRequest.RequesterId && f.Friend1 == friendRequest.ReceiverId));
                if (exisitngFriend != null)
                {
                    // Update existing record
                    exisitngFriend.DeletedAt = null;
                    exisitngFriend.DeletedById = null;
                    exisitngFriend.UpdatedAt = DateTime.Now;
                    exisitngFriend.UpdatedById = userId;
                    await _friendRepository.Update(exisitngFriend);
                }
                else
                {
                    // Add into friends table
                    Friend friend = new Friend
                    {
                        Friend1 = friendRequest.RequesterId,
                        Friend2 = (int)friendRequest.ReceiverId,
                        FriendRequestId = friendRequest.Id,
                        CreatedById = userId,
                        UpdatedAt = DateTime.Now,
                        UpdatedById = userId
                    };
                    await _friendRepository.Add(friend);
                }

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

    public async Task<PaginatedListVM<FriendVM>> FriendList(FilterVM filter, int groupId = 0)
    {
        int currentUserId = _userService.LoggedInUserId();
        filter.SearchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(@"\s+", "").ToLower();

        // Order filter
        Func<IQueryable<Friend>, IOrderedQueryable<Friend>> orderBy = q => q.OrderBy(f => f.Id);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn.ToLower())
            {
                case "name":
                    orderBy = filter.SortOrder == "asc"
                        ? q => q.OrderBy(f => f.Friend1 == currentUserId ? f.Friend2UserNavigation.FirstName : f.Friend1UserNavigation.FirstName)
                        : q => q.OrderByDescending(f => f.Friend1 == currentUserId ? f.Friend2UserNavigation.FirstName : f.Friend1UserNavigation.FirstName);
                    break;
                case "email":
                    orderBy = filter.SortOrder == "asc"
                        ? q => q.OrderBy(f => f.Friend1 == currentUserId ? f.Friend2UserNavigation.EmailAddress : f.Friend1UserNavigation.EmailAddress)
                        : q => q.OrderByDescending(f => f.Friend1 == currentUserId ? f.Friend2UserNavigation.EmailAddress : f.Friend1UserNavigation.EmailAddress);
                    break;
                default:
                    break;
            }
        }

        // Get paginated records
        PaginatedItemsVM<Friend> paginatedItems = await _friendRepository.PaginatedList(
            predicate: f => (f.Friend1 == currentUserId || f.Friend2 == currentUserId)
                && (filter.IsDeleted ? f.DeletedAt != null : f.DeletedAt == null)
                && (f.FriendRequest.Status != FeriendRequestStatus.Requested)
                && (groupId != 0
                    ? (f.Friend1 == currentUserId
                        ? !f.Friend2UserNavigation.GroupMembers.Any(gm => gm.GroupId == groupId && gm.DeletedAt == null)
                        : !f.Friend1UserNavigation.GroupMembers.Any(gm => gm.GroupId == groupId && gm.DeletedAt == null))
                    : true)
                && (string.IsNullOrEmpty(filter.SearchString)
                    || (f.Friend1 == currentUserId
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
                fr => fr.Friend2UserNavigation,
                fr => fr.FriendRequest
            },
            thenIncludes: new List<Func<IQueryable<Friend>, IQueryable<Friend>>>
            {
                q => q.Include(f => f.Friend1UserNavigation)
                .ThenInclude(u => u.GroupMembers),
                q => q.Include(f => f.Friend2UserNavigation)
                .ThenInclude(u => u.GroupMembers)
            },
            pageSize: filter.PageSize,
            pageNumber: filter.PageNumber
        );

        // Friend id list
        List<int> friendUserIds = paginatedItems.Items
        .Select(f => f.Friend1 == currentUserId ? f.Friend2 : f.Friend1)
        .ToList();

        // Calculate net amount
        Dictionary<int, decimal> netAmounts = await (
            from e in _expenseRepository.Query()
            where e.DeletedAt == null
            from es in e.ExpenseShares
            where (e.PaidById == currentUserId && friendUserIds.Contains(es.UserId))
            || (es.UserId == currentUserId && friendUserIds.Contains(e.PaidById))
            group new { e, es } by (e.PaidById == currentUserId ? es.UserId : e.PaidById) into g
            select new
            {
                FriendUserId = g.Key,
                Expense = g.Sum(x => x.e.PaidById == currentUserId ? x.es.ShareAmount : -x.es.ShareAmount)
            }
        ).ToDictionaryAsync(x => x.FriendUserId, x => x.Expense);

        // Set paginatet data to viewmodel
        PaginatedListVM<FriendVM> paginatedList = new PaginatedListVM<FriendVM>();
        paginatedList.List = paginatedItems.Items.Select(f =>
        {
            User friendUser = f.Friend1 == currentUserId ? f.Friend2UserNavigation : f.Friend1UserNavigation;
            decimal netAmount = netAmounts.ContainsKey(friendUser.Id) ? netAmounts[friendUser.Id] : 0;

            return new FriendVM
            {
                FriendId = f.Id,
                UserId = friendUser.Id,
                Name = $"{friendUser.FirstName} {friendUser.LastName}",
                EmailAddress = friendUser.EmailAddress,
                ProfileImagePath = friendUser.ProfileImagePath,
                Expense = netAmount
            };
        }).ToList();

        paginatedList.Page.SetPagination(paginatedItems.TotalRecords, filter.PageSize, filter.PageNumber);
        paginatedList.IsDeletedData = filter.IsDeleted;

        return paginatedList;
    }

    public async Task<ResponseVM> RemoveFriend(int friendId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            User currentUser = await _userService.LoggedInUser();

            ResponseVM response = new ResponseVM();
            Friend friend = await _friendRepository.Get(f => f.Id == friendId);

            int otherUserId = friend.Friend1 == currentUser.Id ? friend.Friend2 : friend.Friend1;

            // Calculate net amount
            decimal netAmount = await _expenseShareRepository.Sum(
                selector: es => es.Expense.PaidById == currentUser.Id ? es.ShareAmount : -es.ShareAmount,
                predicate: es => es.DeletedAt == null && es.Expense.DeletedAt == null
                            && ((es.UserId == currentUser.Id && es.Expense.PaidById == otherUserId)
                                || (es.Expense.PaidById == currentUser.Id && es.UserId == otherUserId)),
                includes: new List<Expression<Func<ExpenseShare, object>>>
                {
                    es => es.Expense
                }
            );

            if (netAmount != 0)
            {
                response.Success = false;
                response.Message = NotificationMessages.SettleBeforeRemove.Replace("{0}", "friend");
            }
            else
            {
                friend.DeletedAt = DateTime.Now;
                friend.DeletedById = currentUser.Id;
                await _friendRepository.Update(friend);

                int friendUserId = friend.Friend1 == currentUser.Id ? friend.Friend2 : friend.Friend1;
                User friendUser = await _userService.GetById(friendUserId);

                // Send email
                await _emailService.FriendRemovedEmail(friendUser.FirstName, $"{currentUser.FirstName} {currentUser.LastName}", friendUser.EmailAddress);

                response.Success = true;
                response.Message = NotificationMessages.FriendRemoved.Replace("{0}", friendUser.FirstName);
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

    public async Task<byte[]> ExportFriends(FilterVM filter)
    {
        PaginatedListVM<FriendVM> paginatedList = await FriendList(filter);
        if (!paginatedList.List.Any())
        {
            return null;
        }
        List<string> columns = new List<string>
        {
            "Name", "EmailAddress", "Expense"
        };
        return ExcelExportHelper.ExportToExcel(paginatedList.List, columns, "Friends");
    }

}
