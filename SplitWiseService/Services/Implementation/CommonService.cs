using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class CommonService : ICommonService
{
    private readonly IGenericRepository<Currency> _currencyRepository;
    public CommonService(IGenericRepository<Currency> currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<List<Currency>> CurrencyList()
    {
        return await _currencyRepository.List();
    }

}
