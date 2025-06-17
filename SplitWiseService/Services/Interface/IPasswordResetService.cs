using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IPasswordResetService
{
    public Task<PasswordResetToken> Get(string token);
    public Task Add(int userId, string token);
    public Task<ResponseVM> Validate(string token);
    public Task SetConsumed(string token);
    public Task<ResponseVM> ResetPassword(PasswordResetVM passwordReset);
    public Task<ResponseVM> ChangePassword(PasswordResetVM passwordReset);
}
