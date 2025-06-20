using SplitWiseRepository.Models;

namespace SplitWiseService.Services.Interface;

public interface ICommonService
{
    public Task<List<Currency>> CurrencyList();
}
