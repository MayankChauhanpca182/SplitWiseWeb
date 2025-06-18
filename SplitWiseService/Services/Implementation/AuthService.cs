using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly ITransactionRepository _transaction;
    private readonly IEmailService _emailService;
    private readonly AesHelper _aesHelper;
    private readonly IJwtService _jwtService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthService(IGenericRepository<User> userRepository, IEmailService emailService, AesHelper aesHelper, IJwtService jwtService, ITransactionRepository transaction, IPasswordResetService passwordResetService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _aesHelper = aesHelper;
        _jwtService = jwtService;
        _transaction = transaction;
        _passwordResetService = passwordResetService;
    }

    public async Task<ResponseVM> UserVerification(string token)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();

            string email = _aesHelper.Decrypt(token);
            User? user = await _userRepository.Get(u => u.EmailAddress == email && u.DeactivatedAt == null);
            ResponseVM response = new();
            if (user == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.Invalid.Replace("{0}", "Token");
            }
            else if (user.IsEmailConfirmed)
            {
                response.Success = true;
                response.Message = NotificationMessages.EmailAlreadyVerified;
            }
            else
            {
                user.IsEmailConfirmed = true;
                user.IsActive = true;
                user.UpdatedAt = DateTime.Now;
                await _userRepository.Update(user);

                response.Success = true;
                response.Message = NotificationMessages.EmailVerificationSuccess;
            }

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

    public async Task<ResponseVM> ValidateUser(string email, string password)
    {
        User? user = await _userRepository.Get(u => u.EmailAddress.ToLower() == email.ToLower() && u.DeactivatedAt == null);
        ResponseVM response = new ResponseVM();

        if (user == null)
        {
            response.Success = false;
            response.Message = NotificationMessages.UserNotFound;
        }
        else if (!user.IsEmailConfirmed)
        {
            response.Success = false;
            response.Message = NotificationMessages.EmailNotVerified;
        }
        else if (PasswordHelper.Verify(password, user.PasswordHash))
        {
            response.Success = true;
            response.StringValue = _jwtService.GenerateToken(user);
        }
        else
        {
            response.Success = false;
            response.Message = NotificationMessages.LoginFailed;
        }

        return response;
    }

    public async Task<ResponseVM> ForgotPassword(string email)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();

            // Fetch user
            User? user = await _userRepository.Get(u => u.EmailAddress.ToLower() == email.ToLower() && u.DeactivatedAt == null);
            if (user == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.UserNotFoundByEmail;
            }
            else if (!user.IsEmailConfirmed)
            {
                response.Success = false;
                response.Message = NotificationMessages.EmailNotVerified;
            }
            else
            {
                // Generate a guid token 
                string token = Guid.NewGuid().ToString();

                // store in DB
                await _passwordResetService.Add(user.Id, token);

                // Send email
                await _emailService.ResetPasswordEmail(user.FirstName, email, token);
                response.Success = true;
                response.Message = NotificationMessages.ResetPasswordEmailSuccess;
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
