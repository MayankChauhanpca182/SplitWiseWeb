namespace SplitWiseRepository.Models;

public class AuditFields
{
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedById { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? DeletedById { get; set; }
    public DateTime? DeletedAt { get; set; }
}
