using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Security;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Currency> _currencyRepository;
    private readonly IGenericRepository<FriendRequest> _friendRequestRepository;
    private readonly IGenericRepository<UserReferral> _userReferralRepository;
    private readonly ITransactionRepository _transaction;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IGenericRepository<User> userRepository, IHttpContextAccessor httpContextAccessor, ITransactionRepository transaction, IGenericRepository<Currency> currencyRepository, IEmailService emailService, IGenericRepository<FriendRequest> friendRequestRepository, IGenericRepository<UserReferral> userReferralRepository)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _transaction = transaction;
        _currencyRepository = currencyRepository;
        _emailService = emailService;
        _friendRequestRepository = friendRequestRepository;
        _userReferralRepository = userReferralRepository;
    }

    #region Get user
    public async Task<User> GetByEmailAddress(string email)
    {
        return await _userRepository.Get(u => u.EmailAddress.ToLower() == email.ToLower());
    }

    public async Task<User> GetById(int userId)
    {
        return await _userRepository.Get(u => u.Id == userId && u.IsActive);
    }
    #endregion

    #region Register
    public async Task<ResponseVM> RegisterUser(RegisterUserVM registerUserVM)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new();

            // Check existing user
            User? existingUser = await _userRepository.Get(u => u.EmailAddress == registerUserVM.Email);
            if (existingUser != null)
            {
                response.Success = false;
                response.Message = NotificationMessages.EmailExists.Replace("{0}", registerUserVM.Email);
                return response;
            }

            //  Create User Instance
            User newUser = new()
            {
                FirstName = registerUserVM.FirstName,
                LastName = registerUserVM.LastName,
                EmailAddress = registerUserVM.Email,
                PasswordHash = PasswordHelper.Hash(registerUserVM.Password),
                CurrencyId = 1,
                ProfileImagePath = ImageHelper.GetRandomImage(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Add User
            await _userRepository.Add(newUser);

            // Send Email
            await _emailService.UserVarificationEmail(newUser.FirstName, newUser.EmailAddress);

            response.Success = true;
            response.Message = NotificationMessages.RegisterSuccess;

            // Commit transaction
            await _transaction.Commit();
            return response;
        }
        catch
        {
            // Rollbak transaction
            await _transaction.Rollback();
            throw;
        }
    }
    #endregion

    #region Profile
    public async Task<UserVM> GetProfile()
    {
        User user = await LoggedInUser();
        return new UserVM
        {
            EmailAddress = user.EmailAddress,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfileImagePath = user.ProfileImagePath,
            CurrencyId = user.CurrencyId,
            Address = user.Address,
            Birthdate = user.Birthdate,
            Currencies = await _currencyRepository.List()
        };
    }

    public async Task<ResponseVM> Update(UserVM newUser)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();

            // Current user id
            User originalUser = await LoggedInUser();

            originalUser.FirstName = newUser.FirstName;
            originalUser.LastName = newUser.LastName;
            originalUser.Address = newUser.Address;
            originalUser.Birthdate = newUser.Birthdate;
            originalUser.CurrencyId = newUser.CurrencyId;

            // Image upload
            if (newUser.ProfileImage != null)
            {
                originalUser.ProfileImagePath = ImageHelper.UploadImage(newUser.ProfileImage, originalUser.ProfileImagePath);
            }

            await _userRepository.Update(originalUser);
            response.Success = true;
            response.Message = string.Format(NotificationMessages.Saved, "Profile");

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
    #endregion

    #region Get Logged In User
    public int LoggedInUserId()
    {
        Claim userIdClaim = _httpContextAccessor.HttpContext.User?.FindFirst("id");
        return int.Parse(userIdClaim?.Value);
    }

    public async Task<User> LoggedInUser()
    {
        int userId = LoggedInUserId();
        return await GetById(userId);
    }
    #endregion

}
