using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    public AuthService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseVM> RegisterUser(RegisterUserVM registerUserVM)
    {
        //  Create User Instance
        User user = new()
        {
            FirstName = registerUserVM.FirstName,
            LastName = registerUserVM.LastName,
            EmailAddress = registerUserVM.Email,
            PasswordHash = PasswordHelper.Hash(registerUserVM.Password),
            CurrencyId = 1
        };

        // Add User
        await _userRepository.Add(user);

        // Send Email
        

        return new();
    }

}
