using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class GroupActivity : AuditFields
{
    public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    public int? GroupId { get; set; }
    [ForeignKey("GroupId")]
    public virtual Group Group { get; set; }
    
    [MaxLength(500)]
    public string Activity { get; set; }
}
