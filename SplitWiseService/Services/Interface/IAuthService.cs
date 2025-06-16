using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IAuthService
{
    public Task<ResponseVM> RegisterUser(RegisterUserVM registerUserVM);
    public Task<ResponseVM> UserVerification(string token);
    public Task<ResponseVM> ValidateUser(string email, string password);
    public Task<ResponseVM> ForgotPassword(string email);
    public Task<ResponseVM> ResetPassword(PasswordResetVM passwordReset);
}
