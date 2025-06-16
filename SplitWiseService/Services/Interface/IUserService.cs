using SplitWiseRepository.Models;

namespace SplitWiseService.Services.Interface;

public interface IUserService
{
    public Task<User?> GetByEmailAddress(string email);
    public Task<User?> GetById(int userId);
    public Task ChangePassword(int userId, string newPassword);

    // Get current user id
    public Task<int> LoggedInUserId();
}
