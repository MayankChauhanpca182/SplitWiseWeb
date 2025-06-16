using System.ComponentModel.DataAnnotations;

namespace SplitWiseRepository.Models;

public class PasswordResetToken
{
    public int Id { get; set; }

    [MaxLength(250)]
    public string EmailAddress { get; set; }


    [MaxLength(500)]
    public string Token { get; set; }

    public bool IsUsed { get; set; } = false;
    public DateTime ExpireAt { get; set; } = DateTime.Now.AddHours(6);
}
