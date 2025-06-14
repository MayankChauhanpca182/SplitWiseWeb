using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IGenericRepository<User> userRepository, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<User?> GetByEmailAddress(string email)
    {
        return await _userRepository.Get(u => u.EmailAddress.ToLower() == email.ToLower());
    }

    #region Get Logged In User
    public async Task<int?> LoggedInUserId()
    {
        string idStr = _jwtService.GetClaimValue(_httpContextAccessor.HttpContext?.Request.Cookies["JwtToken"]!, "id")!;
        if (!string.IsNullOrEmpty(idStr) && int.TryParse(idStr, out int userId))
        {
            return userId;
        }
        else
        {
            return (int?)null;
        }
        
    }
    #endregion

}
