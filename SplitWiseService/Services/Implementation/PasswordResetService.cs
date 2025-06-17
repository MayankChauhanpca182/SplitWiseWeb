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

}
