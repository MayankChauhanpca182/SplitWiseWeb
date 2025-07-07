using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class ActivityService : IActivityService
{
    private readonly IGenericRepository<UserActivity> _userActivityService;
    private readonly IGenericRepository<GroupActivity> _groupActivityService;
    private readonly IUserService _userService;

    public ActivityService()
    {
    }

}
