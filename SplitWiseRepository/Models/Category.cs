using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class Category : AuditFields
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    public string CategoryImagePath { get; set; }
    public bool IsSystem { get; set; } = false;
    public int? GroupId { get; set; }

    [ForeignKey("GroupId")]
    public virtual Group Group { get; set; }
}
