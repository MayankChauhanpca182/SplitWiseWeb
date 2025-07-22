using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface ISettlementService
{
    public Task<SettleUpListVM> SettleUpList(int friendUserId);
    public Task<ResponseVM> AddSettlement(SettlementVM settlement);
}
