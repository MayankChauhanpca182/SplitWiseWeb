using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepository;

    public CategoryService(IGenericRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Category> GetById(int categoryId)
    {
        return await _categoryRepository.Get(
            predicate: c => c.DeletedAt == null && c.Id == categoryId
        );
    }

    public async Task<List<Category>> GetList()
    {
        List<Category> categories = await _categoryRepository.List(
            predicate: c => c.IsSystem,
            orderBy: c => c.OrderBy(c => c.Name)
        );
        return categories;
    }

}
