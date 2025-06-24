using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUserService _userService;

    public CategoryService(IGenericRepository<Category> categoryRepository, IUserService userService)
    {
        _categoryRepository = categoryRepository;
        _userService = userService;
    }

    public async Task<List<Category>> GetList()
    {
        int currentUserId = _userService.LoggedInUserId();
        List<Category> categories = await _categoryRepository.List(
            predicate: c => c.IsSystem
        );
        return categories;
    }

}
