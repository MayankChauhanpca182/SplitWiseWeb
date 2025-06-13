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

    public AuthService(IGenericRepository<User> userRepository, IEmailService emailService, AesHelper aesHelper, IJwtService jwtService, ITransactionRepository transaction)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _aesHelper = aesHelper;
        _jwtService = jwtService;
        _transaction = transaction;
    }

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
                response.Message = NotificationMessages.EmailNotConfirmed;
            }
            else
            {
                user.IsEmailConfirmed = true;
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
            response.Token = _jwtService.GenerateToken(user);
            response.Name = $"{user.FirstName} {user.LastName}";
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
        User? user = await _userRepository.Get(u => u.EmailAddress.ToLower() == email.ToLower() && u.DeactivatedAt == null);
        ResponseVM response = new ResponseVM();

        if (user == null)
        {
            response.Success = false;
            response.Message = NotificationMessages.UserNotFound;
        }
        else
        {
            // Send email
            await _emailService.ResetPasswordEmail(email);
            response.Success = true;
            // response.Message = NotificationMessages.;
        }

        return response;
    }
}
