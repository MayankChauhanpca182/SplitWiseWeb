using System.Security.Claims;
using SplitWiseRepository.Models;

namespace SplitWiseService.Services.Interface;

public interface IJwtService
{
    public string GenerateToken(User user, bool IsRememberMe);
    public string GetClaimValue(string token, string claimType);
}
