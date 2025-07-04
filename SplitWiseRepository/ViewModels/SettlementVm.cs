using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class SettlementVM
{
    public int GroupId { get; set; } = 0;
    public int PaidById { get; set; } = 0;
    public int PaidToId { get; set; } = 0;
    public User PaidToUser { get; set; }

    [Required(ErrorMessage = ValidationMessages.ExpenseAmountRequired)]
    public decimal Amount { get; set; } = 0;

    public IFormFile Attachment { get; set; }

    [StringLength(1000)]
    public string Note { get; set; }

    [Required(ErrorMessage = ValidationMessages.CurrencyRequired)]
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.CurrencyRequired)]
    public int CurrencyId { get; set; } = DefaultValues.CurrencyId;
    public List<Currency> Currencies { get; set; } = new List<Currency>();

    public bool SettleAll { get; set; } = false;
}
