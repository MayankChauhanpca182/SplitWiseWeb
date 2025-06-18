using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
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
}
