using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;

    public UserService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetByEmailAddress(string email)
    {
        return await _userRepository.Get(u => u.EmailAddress.ToLower() == email.ToLower());
    }

}
