using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Helpers;
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

    public async Task<User?> GetById(int userId)
    {
        return await _userRepository.Get(u => u.Id == userId && u.IsActive);
    }

    public async Task ChangePassword(int userId, string newPassword)
    {
        User user = await _userRepository.Get(u => u.Id == userId);
        user.PasswordHash = PasswordHelper.Hash(newPassword);
        user.UpdatedAt = DateTime.Now;
        await _userRepository.Update(user);
        return;
    }

    #region Get Logged In User
    public async Task<int> LoggedInUserId()
    {
        Claim userIdClaim = _httpContextAccessor.HttpContext.User?.FindFirst("id");
        return int.Parse(userIdClaim?.Value);
    }
    #endregion

}
