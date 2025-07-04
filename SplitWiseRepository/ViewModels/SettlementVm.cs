using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class SettlementVm
{
    public decimal Amount { get; set; } = 0;
    public IFormFile Attachment { get; set; }

    public int CurrencyId { get; set; } = DefaultValues.CurrencyId;
    public List<Currency> Currencies { get; set; } = new List<Currency>();
}
