using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IAuthService
{
    public Task<ResponseVM> RegisterUser(RegisterUserVM registerUserVM);
    public Task<ResponseVM> UserVarification(string token);
    public Task<ResponseVM> ValidateUser(string email, string password);
}
