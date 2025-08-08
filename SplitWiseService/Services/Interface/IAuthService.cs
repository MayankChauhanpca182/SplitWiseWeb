using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IAuthService
{
    public Task<ResponseVM> UserVerification(string token);
    public Task<ResponseVM> ValidateUser(string email, string password, bool IsRememberMe);
    public Task<ResponseVM> ForgotPassword(string email);
}
