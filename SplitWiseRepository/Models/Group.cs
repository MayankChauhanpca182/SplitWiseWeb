using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.Models;

public class Group : AuditFields
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string NoticeBoard { get; set; }

    public int CurrencyId { get; set; }
    public bool IsSimplifiedPayments { get; set; } = false;

    [MaxLength(1000)]
    public string ImagePath { get; set; }

    [ForeignKey("CurrencyId")]
    public virtual Currency Currency { get; set; }

    public ICollection<GroupMember> GroupMembers { get; set; }
}
