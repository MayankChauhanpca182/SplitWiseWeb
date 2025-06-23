using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class ExpenseShare : AuditFields
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    [ForeignKey("ExpenseId")]
    public virtual Expense Expense { get; set; }

    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [Column(TypeName ="decimal(10,2)")]
    public decimal ShareAmount { get; set; } = 0;
}
