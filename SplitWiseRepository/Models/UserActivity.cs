using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class UserActivity : AuditFields
{
    public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [MaxLength(500)]
    public string Activity { get; set; }
}
