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
    public string Name { get; set; }

    [ImageType]
    public IFormFile Image { get; set; }
    public string ImagePath { get; set; }

    [StringLength(2000)]
    public string NoticeBoard { get; set; }
    public int CurrencyId { get; set; } = 23;
    public bool IsSimplifiedPayments { get; set; } = false;

    public decimal Expense { get; set; } = 0;
    public bool IsSettled { get; set; }

    public List<Currency> Currencies { get; set; } = new List<Currency>();
    public List<GroupMemberVM> Members { get; set; } = new List<GroupMemberVM>();
}
