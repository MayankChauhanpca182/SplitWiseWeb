using System.ComponentModel.DataAnnotations;
using SplitWiseRepository.Constants;

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
    public DateTime ExpireAt { get; set; } = DateTime.Now.AddHours(DefaultValues.TokenExpireHours);

    public virtual User User { get; set; }
}
