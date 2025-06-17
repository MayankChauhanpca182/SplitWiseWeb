using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IUserService
{
    // Get user
    public Task<User> GetByEmailAddress(string email);
    public Task<User> GetById(int userId);

    // Register
    public Task<ResponseVM> RegisterUser(RegisterUserVM registerUserVM);

    // Profile
    public Task<UserVM> GetProfile();
    public Task<ResponseVM> Update(UserVM profile);

    // Get logged in user 
    public int LoggedInUserId();
    public Task<User> LoggedInUser();
}
