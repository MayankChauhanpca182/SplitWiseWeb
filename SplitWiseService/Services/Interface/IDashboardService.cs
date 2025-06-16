using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IDashboardService
{
    public Task<ResponseVM> ChangePassword(PasswordResetVM passwordReset);
}