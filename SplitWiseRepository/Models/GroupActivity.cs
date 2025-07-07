using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.Models;

public class GroupActivity : AuditFields
{
    public int Id { get; set; }

    public ActivityType ActivityType { get; set; }

    public int PerformedById { get; set; }
    [ForeignKey("PerformedById")]
    public virtual User PerformedByUser { get; set; }

    public int? PerformedOnId { get; set; }
    [ForeignKey("PerformedOnId")]
    public virtual User PerformedOnUser { get; set; }

    public int? GroupId { get; set; }
    [ForeignKey("GroupId")]
    public virtual Group Group { get; set; }

    public int? ExpenseId { get; set; }
    [ForeignKey("ExpenseId")]
    public virtual Expense Expense { get; set; }

    public int? PaymentId { get; set; }
    [ForeignKey("PaymentId")]
    public virtual Payment Payment { get; set; }
}
