using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class DashboardService : IDashboardService
{
    public DashboardService()
    {
    }

    public async Task<DashboardVM> GetDashboard()
    {
        
        return new DashboardVM();
    }

}
