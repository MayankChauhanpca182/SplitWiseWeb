using System.ComponentModel.DataAnnotations;

namespace SplitWiseRepository.Models;

public class PasswordResetToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Token { get; set; }
    public bool IsConsumed { get; set; } = false;
    public DateTime? ConsumedAt { get; set; }
    public DateTime ExpireAt { get; set; } = DateTime.Now.AddHours(6);

    public virtual User User { get; set; }
}
