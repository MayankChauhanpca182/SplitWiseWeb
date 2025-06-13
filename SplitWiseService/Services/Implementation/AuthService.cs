using System;
using Microsoft.Extensions.DependencyModel.Resolution;
using Org.BouncyCastle.Tls;
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
                response.Message = NotificationMessages.Exist.Replace("{0}", $"Account with email {registerUserVM.Email}");
                return response;
            }

            //  Create User Instance
            User newUser = new()
            {
                FirstName = registerUserVM.FirstName,
                LastName = registerUserVM.LastName,
                EmailAddress = registerUserVM.Email,
                PasswordHash = PasswordHelper.Hash(registerUserVM.Password),
                CurrencyId = 1
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

    public async Task<ResponseVM> UserVarification(string token)
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
                response.Message = "Invalid Token.";
            }
            else if (user.IsEmailConfirmed)
            {
                response.Success = true;
                response.Message = "Email is already verified.";
            }
            else
            {
                user.IsEmailConfirmed = true;
                await _userRepository.Update(user);
                response.Success = true;
                response.Message = "Email verified successfully.";
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
        User? user = await _userRepository.Get(u => u.EmailAddress == email && u.DeactivatedAt == null);
        ResponseVM response = new();
        if (user == null)
        {
            response.Success = false;
            response.Message = "User not found.";
        }
        else if (!user.IsEmailConfirmed)
        {
            response.Success = false;
            response.Message = "Email is not verified.";
        }
        else if (PasswordHelper.Verify(password, user.PasswordHash))
        {
            response.Success = true;
            response.Message = $"Hello {user.FirstName}";
            response.Token = _jwtService.GenerateToken(user);
            response.Name = user.FirstName;
        }
        else
        {
            response.Success = false;
            response.Message = "Invalid credentials.";
        }

        return response;
    }

}
