using SplitWiseRepository.Models;

namespace SplitWiseService.Services.Interface;

public interface IUserService
{
    public Task<User?> GetByEmailAddress(string email);
}
