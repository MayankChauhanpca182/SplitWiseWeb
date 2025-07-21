using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Attributes;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class GroupVM
{
    public int Id { get; set; } = 0;

    [Required(ErrorMessage = ValidationMessages.GroupName)]
    [StringLength(50, ErrorMessage = ValidationMessages.FirstNameLength)]
    [RegularExpression(ValidationRegex.GroupNameRegex, ErrorMessage = ValidationMessages.ValidGroupName)]
    [ExcelColumn("Name")]
    public string Name { get; set; }

    [ImageType]
    public IFormFile Image { get; set; }
    public string ImagePath { get; set; }

    [StringLength(2000)]
    [ExcelColumn("Notice")]
    public string NoticeBoard { get; set; }
    public int CurrencyId { get; set; } = DefaultValues.CurrencyId;
    public bool IsSimplifiedPayments { get; set; } = false;

    [ExcelColumn("Expense")]
    public decimal Expense { get; set; } = 0;
    public bool IsSettled { get; set; }

    public List<Currency> Currencies { get; set; } = new List<Currency>();
    public List<GroupMemberVM> Members { get; set; } = new List<GroupMemberVM>();
    public ActivityFilterVM ActivityFilter { get; set; } = new ActivityFilterVM();

    public decimal BorrowedAmount { get; set; } = 0;
    public decimal LentAmount { get; set; } = 0;
}
