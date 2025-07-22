using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface ISettlementService
{
    public Task<SettleUpListVM> SettleUpList(int friendUserId);
    public Task<ResponseVM> AddSettlement(SettlementVM settlement);
    public Task<PaginatedListVM<SettlementListVM>> SettlementList(FilterVM filter);
    public Task<byte[]> ExportSettlements(FilterVM filter);
}
