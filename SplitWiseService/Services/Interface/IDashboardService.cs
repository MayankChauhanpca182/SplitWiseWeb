using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IDashboardService
{
    public Task<ResponseVM> ChangePassword(PasswordResetVM passwordReset);
    public Task<ProfileVM> GetProfile();
    public Task<ResponseVM> UpdateProfile(ProfileVM profile);
}