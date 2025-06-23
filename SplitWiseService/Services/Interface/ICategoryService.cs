using SplitWiseRepository.Models;

namespace SplitWiseService.Services.Interface;

public interface ICategoryService
{
    public Task<List<Category>> GetList(int? groupId);
}
