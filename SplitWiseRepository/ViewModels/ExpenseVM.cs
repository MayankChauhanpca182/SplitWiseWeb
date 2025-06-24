using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Attributes;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class ExpenseVM
{
    public int Id { get; set; } = 0;
    public int? GroupId { get; set; }

    [Required(ErrorMessage = ValidationMessages.ExpenseTitleRequired)]
    [StringLength(100, ErrorMessage = ValidationMessages.ExpenseTitleRequired)]
    public string Title { get; set; }

    [Required(ErrorMessage = ValidationMessages.ExpenseAmountRequired)]
    public string Amount { get; set; } = "0";

    public int CategoryId { get; set; } = 16;
    public List<Category> Categories { get; set; } = new List<Category>();

    [Required(ErrorMessage = ValidationMessages.CurrencyRequired)]
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.CurrencyRequired)]
    public int CurrencyId { get; set; } = 23;
    public List<Currency> Currencies { get; set; } = new List<Currency>();

    [StringLength(1000)]
    public string Note { get; set; }

    public int PaidById { get; set; }

    [Required(ErrorMessage = ValidationMessages.PaymentDateRequired)]
    [DataType(DataType.Date)]
    [NoFutureDate(ErrorMessage = ValidationMessages.NoFuturePaymentDate)]
    public DateTime PaidDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = ValidationMessages.SplitTypeRequired)]
    public SplitType SplitTypeEnum { get; set; } = 0;

    public IFormFile Attachment { get; set; }

    public List<FriendVM> Friends { get; set; } = new List<FriendVM>();
    public List<ExpenseShareVM> ExpenseShares { get; set; } = new List<ExpenseShareVM>();

    public decimal NetAmount { get; set; } = 0;
    public bool IsViewOnly { get; set; } = false;
}
