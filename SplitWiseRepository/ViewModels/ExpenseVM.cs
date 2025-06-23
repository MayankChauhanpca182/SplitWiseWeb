using System.ComponentModel.DataAnnotations;
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
    [Range(0, double.MaxValue, ErrorMessage = ValidationMessages.ValidExpenseAmount)]
    public decimal Amount { get; set; } = 0;

    public int CategoryId { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();

    [Required(ErrorMessage = ValidationMessages.CurrencyRequired)]
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.CurrencyRequired)]
    public int CurrencyId { get; set; }
    public List<Currency> Currencies { get; set; } = new List<Currency>();

    public int PaidById { get; set; }

    [Required(ErrorMessage = ValidationMessages.PaymentDateRequired)]
    [DataType(DataType.Date)]
    [NoFutureDate(ErrorMessage = ValidationMessages.NoFuturePaymentDate)]
    public DateTime? PaymentDate { get; set; }

    [Required(ErrorMessage = ValidationMessages.SplitTypeRequired)]
    public SplitType SplitType { get; set; } = 0;

    public List<ExpenseShareVM> ExpenseShares { get; set; } = new List<ExpenseShareVM>();
}
