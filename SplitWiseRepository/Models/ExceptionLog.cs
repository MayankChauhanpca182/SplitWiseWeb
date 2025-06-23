using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class ExceptionLog
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(500)]
    public string APIEndPoint { get; set; }

    [Required]
    public string ExceptionMessage { get; set; }
    public string InnerException { get; set; }
    public int? UserId { get; set; }
    public int? GroupId { get; set; }
    public int? ExpenseId { get; set; }
    public DateTime ExceptionAt { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(100)]
    public string MachineName { get; set; }
    // public virtual User User { get; set; } = null!;
}
