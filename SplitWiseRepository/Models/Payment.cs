using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class Payment : AuditFields
{
    public int Id { get; set; }
    public int PaidById { get; set; }
    [ForeignKey("PaidById")]
    public User PaidByUser { get; set; }

    public int PaidToId { get; set; }
    [ForeignKey("PaidToId")]
    public User PaidToUser { get; set; }

    public int CurrencyId { get; set; }
    [ForeignKey("CurrencyId")]
    public Currency Currency { get; set; }

    public decimal Amount { get; set; }

    public string AttachmentPath { get; set; }
    public string AttachmentName { get; set; }
}
