using Microsoft.AspNetCore.Authorization;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class PasswordResetService : IPasswordResetService
{
    private readonly IGenericRepository<PasswordResetToken> _passwordResetToken;

    public PasswordResetService(IGenericRepository<PasswordResetToken> passwordResetToken)
    {
        _passwordResetToken = passwordResetToken;
    }

    public async Task<PasswordResetToken?> Get(string token)
    {
        return await _passwordResetToken.Get(predicate: prt => prt.Token == token && !prt.IsUsed);
    }

    public async Task Add(string email, string token)
    {
        PasswordResetToken resetToken = new PasswordResetToken
        {
            EmailAddress = email,
            Token = token
        };
        await _passwordResetToken.Add(resetToken);
        return;
    }

    public async Task<ResponseVM> Validate(string token)
    {
        ResponseVM response = new ResponseVM();

        // Fetch password reset token token from DB and validate 
        PasswordResetToken? resetToken = await Get(token);
        if (resetToken == null)
        {
            response.Success = false;
            response.Message = NotificationMessages.Invalid.Replace("{0}", "reset link");
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
                response.Message = NotificationMessages.LinkExpired;
            }
        }
        return response;
    }

    public async Task SetUsed(string token)
    {
        // Fetch password reset token token from DB
        PasswordResetToken? resetToken = await Get(token);
        resetToken.IsUsed = true;
        await _passwordResetToken.Update(resetToken);
        return;
    }

}
