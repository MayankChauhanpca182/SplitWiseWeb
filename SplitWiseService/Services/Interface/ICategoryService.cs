using SplitWiseRepository.Models;

namespace SplitWiseService.Services.Interface;

public interface ICategoryService
{
    public Task<Category> GetById(int categoryId);
    public Task<List<Category>> GetList();
}
