using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.Models;
public class Expense : AuditFields
{
    public int Id { get; set; }
    public int? GroupId { get; set; }

    [ForeignKey("GroupId")]
    public virtual Group Group { get; set; }


    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; } = 0;

    public int PaidById { get; set; }
    [ForeignKey("PaidById")]
    public virtual User PaidByUser { get; set; }

    public DateTime PaidDate { get; set; }

    public int ExpenseCategoryId { get; set; }
    [ForeignKey("ExpenseCategoryId")]
    public virtual Category ExpenseCategory { get; set; }

    public int CurrencyId { get; set; }
    [ForeignKey("CurrencyId")]
    public virtual Currency Currency { get; set; }

    public SplitType SplitType { get; set; }

    [MaxLength(1000)]
    public string Note { get; set; }
    public string AttachmentPath { get; set; }

    public string AttachmentName { get; set; }
    

    public virtual ICollection<ExpenseShare> ExpenseShares { get; set; }

}
