using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IPasswordResetService
{
    public Task<PasswordResetToken?> Get(string token);
    public Task Add(string email, string token);
    public Task<ResponseVM> Validate(string token);
    public Task SetUsed(string token);
}
