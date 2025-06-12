using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IAuthService
{
    public Task<ResponseVM> RegisterUser(RegisterUserVM registerUserVM);
}
