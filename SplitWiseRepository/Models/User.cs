using System.ComponentModel.DataAnnotations;

namespace SplitWiseRepository.Models;

public class User : AuditFields
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(250)]
    public string EmailAddress { get; set; }

    [Required]
    [MaxLength(1000)]
    public string PasswordHash { get; set; }

    [MaxLength(1000)]
    public string ProfileImagePath { get; set; }

    public bool IsEmailConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = false;
    public int CurrencyId { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    [MaxLength(500)]
    public string Address { get; set; }
    public DateTime? Birthdate { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; }
}
