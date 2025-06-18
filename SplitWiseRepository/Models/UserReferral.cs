using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace SplitWiseRepository.Models;

public class UserReferral
{
    public int Id { get; set; }
    public int ReferredFromUserId { get; set; }

    [Required]
    public string ReferredFromEmailAddress { get; set; }

    [Required]
    public string ReferredToEmailAddress { get; set; }

    public DateTime ReferredAt { get; set; } = DateTime.Now;
    public bool IsAccountRegistered { get; set; } = false;
    public DateTime? RegisteredAt { get; set; }
}
