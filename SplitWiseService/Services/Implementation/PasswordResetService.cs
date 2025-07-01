using Microsoft.AspNetCore.Authorization;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class PasswordResetService : IPasswordResetService
{
    private readonly IGenericRepository<PasswordResetToken> _passwordResetToken;
    private readonly IGenericRepository<User> _userRepository;
    private readonly ITransactionRepository _transaction;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public PasswordResetService(IGenericRepository<PasswordResetToken> passwordResetToken, ITransactionRepository transaction, IUserService userService, IGenericRepository<User> userRepository, IEmailService emailService)
    {
        _passwordResetToken = passwordResetToken;
        _transaction = transaction;
        _userService = userService;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<PasswordResetToken> Get(string token)
    {
        return await _passwordResetToken.Get(predicate: prt => prt.Token == token);
    }

    public async Task Add(int userId, string token)
    {
        PasswordResetToken existingToken = await _passwordResetToken.Get(predicate: prt => prt.UserId == userId && !prt.IsConsumed && prt.ConsumedAt == null);
        if (existingToken != null)
        {
            existingToken.Token = token;
            existingToken.ExpireAt = DateTime.Now.AddHours(6);
            await _passwordResetToken.Update(existingToken);
        }
        else
        {
            PasswordResetToken resetToken = new PasswordResetToken
            {
                UserId = userId,
                Token = token,
                ExpireAt = DateTime.Now.AddHours(6)
            };
            await _passwordResetToken.Add(resetToken);
        }
        return;
    }

    public async Task<ResponseVM> Validate(string token)
    {
        ResponseVM response = new ResponseVM();

        // Fetch password reset token token from DB and validate 
        PasswordResetToken resetToken = await Get(token);
        if (resetToken == null)
        {
            response.Success = false;
            response.Message = NotificationMessages.WrongResetPasswordLink;
        }
        else if (resetToken.IsConsumed)
        {
            response.Success = false;
            response.Message = NotificationMessages.ConsumedResetPasswordLink;
        }
        else
        {
            if (resetToken.ExpireAt.Subtract(DateTime.Now).Ticks > 0)
            {
                response.Success = true;
            }
            else
            {
                response.Success = false;
                response.Message = NotificationMessages.ExpiredResetPasswordLink;
            }
        }
        return response;
    }

    public async Task SetConsumed(string token)
    {
        // Fetch password reset token token from DB
        PasswordResetToken resetToken = await Get(token);
        resetToken.IsConsumed = true;
        resetToken.ConsumedAt = DateTime.Now;
        await _passwordResetToken.Update(resetToken);
        return;
    }

    private async Task SetPassword(int userId, string newPassword)
    {
        User user = await _userRepository.Get(u => u.Id == userId);
        user.PasswordHash = PasswordHelper.Hash(newPassword);
        user.UpdatedAt = DateTime.Now;
        await _userRepository.Update(user);
        return;
    }

    public async Task<ResponseVM> ResetPassword(PasswordResetVM passwordReset)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();

            // Fetch reset token
            PasswordResetToken? resetToken = await Get(passwordReset.ResetToken);
            if (resetToken == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.WrongResetPasswordLink;
                return response;
            }

            // Fetch user
            User? user = await _userRepository.Get(u => u.Id == resetToken.UserId && u.DeactivatedAt == null && u.IsEmailConfirmed && u.DeletedAt == null);
            if (user == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.UserNotFoundByEmail;
            }
            else
            {
                // Set reset token used
                await SetConsumed(resetToken.Token);

                // Update password
                await SetPassword(user.Id, passwordReset.NewPassword);

                // Send email notification
                await _emailService.PasswordChangedEmail(user.FirstName, user.EmailAddress);

                response.Success = true;
                response.Message = NotificationMessages.PasswordResetSuccess;
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

    public async Task<ResponseVM> ChangePassword(PasswordResetVM passwordReset)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            ResponseVM response = new ResponseVM();
            int userId = _userService.LoggedInUserId();
            if (userId == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.UnauthorizeUser;
                return response;
            }

            User user = await _userService.GetById(userId);
            if (PasswordHelper.Verify(passwordReset.Password, user.PasswordHash))
            {
                if (PasswordHelper.Verify(passwordReset.NewPassword, user.PasswordHash))
                {
                    response.Success = false;
                    response.Message = NotificationMessages.CannotSetSamePassword;
                }
                else
                {
                    // Set new password
                    await SetPassword(userId, passwordReset.NewPassword);

                    // Send email notification
                    await _emailService.PasswordChangedEmail(user.FirstName, user.EmailAddress);

                    response.Success = true;
                    response.Message = NotificationMessages.PasswordChangeSuccess;
                }
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
