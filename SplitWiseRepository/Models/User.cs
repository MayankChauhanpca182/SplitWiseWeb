using System.ComponentModel.DataAnnotations;

namespace SplitWiseRepository.Models;

public class User : AuditFields
{
    public int Id { get; set; }

    [MaxLength(50)]
    public String FirstName { get; set; }

    [MaxLength(50)]
    public String LastName { get; set; }

    [MaxLength(250)]
    public String EmailAddress { get; set; }

    [MaxLength(1000)]
    public String PasswordHash { get; set; }

    [MaxLength(1000)]
    public String ProfileImagePath { get; set; }

    public bool IsEmailConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int CurrencyId { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
