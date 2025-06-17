using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class DashboardService : IDashboardService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Currency> _currencyRepository;
    private readonly ITransactionRepository _transaction;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public DashboardService(IGenericRepository<User> userRepository, IUserService userService, IEmailService emailService, ITransactionRepository transaction, IGenericRepository<Currency> currencyRepository)
    {
        _userRepository = userRepository;
        _userService = userService;
        _emailService = emailService;
        _transaction = transaction;
        _currencyRepository = currencyRepository;

    }

    public async Task<ResponseVM> ChangePassword(PasswordResetVM passwordReset)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            ResponseVM response = new ResponseVM();
            int userId = await _userService.LoggedInUserId();
            if (userId == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.UnauthorizeUser;
                return response;
            }

            User user = await _userService.GetById(userId);
            if (PasswordHelper.Verify(passwordReset.Password, user.PasswordHash))
            {
                user.PasswordHash = PasswordHelper.Hash(passwordReset.NewPassword);
                user.UpdatedAt = DateTime.Now;
                await _userRepository.Update(user);

                // Send email notification
                _emailService.ChangePasswordEmail(user.FirstName, user.EmailAddress);

                response.Success = true;
                response.Message = NotificationMessages.PasswordChangeSuccess;
            }
            else
            {
                response.Success = false;
                response.Message = NotificationMessages.IncorrectPassword;
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

    public async Task<ProfileVM> GetProfile()
    {
        int userId = await _userService.LoggedInUserId();

        User user = await _userService.GetById(userId);
        return new ProfileVM
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

    public async Task<ResponseVM> UpdateProfile(ProfileVM profile)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();

            // Current user id
            int userId = await _userService.LoggedInUserId();
            User user = await _userService.GetById(userId);

            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.Address = profile.Address;
            user.Birthdate = profile.Birthdate;
            user.CurrencyId = profile.CurrencyId;

            // Image upload
            if (profile.ProfileImage != null)
            {
                ImageHelper.DeleteImage(user.ProfileImagePath);
                user.ProfileImagePath = ImageHelper.UploadImage(profile.ProfileImage);
            }

            await _userRepository.Update(user);
            response.Success = true;
            response.Message = NotificationMessages.ProfileUpdateSuccess;

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
