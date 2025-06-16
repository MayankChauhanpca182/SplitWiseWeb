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
    private readonly ITransactionRepository _transaction;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public DashboardService(IGenericRepository<User> userRepository, IUserService userService, IEmailService emailService, ITransactionRepository transaction)
    {
        _userRepository = userRepository;
        _userService = userService;
        _emailService = emailService;
        _transaction = transaction;
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

}
