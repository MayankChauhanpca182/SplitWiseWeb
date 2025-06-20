using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class GroupMember : AuditFields
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }

    [ForeignKey("GroupId")]
    public virtual Group Group { get; set; } = new Group();

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = new User();
}
