using System.ComponentModel.DataAnnotations;

namespace SplitWiseRepository.Models;

public class User : AuditFields
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string LastName { get; set; }

    [MaxLength(250)]
    public string EmailAddress { get; set; }

    [MaxLength(1000)]
    public string PasswordHash { get; set; }

    [MaxLength(1000)]
    public string? ProfileImagePath { get; set; }

    public bool IsEmailConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int CurrencyId { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
