using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class Payment : AuditFields
{
    public int Id { get; set; }
    public int PaidById { get; set; }
    [ForeignKey("PaidById")]
    public virtual User PaidByUser { get; set; }

    public int PaidToId { get; set; }
    [ForeignKey("PaidToId")]
    public virtual User PaidToUser { get; set; }

    public int CurrencyId { get; set; }
    [ForeignKey("CurrencyId")]
    public virtual Currency Currency { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    public string AttachmentPath { get; set; }
    public string AttachmentName { get; set; }
}
