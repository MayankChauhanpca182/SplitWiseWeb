using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface ISettlementService
{
    public Task<SettlementListVM> GetList(int friendUserId);
    public Task<ResponseVM> AddSettlement(SettlementVM settlement);
}
