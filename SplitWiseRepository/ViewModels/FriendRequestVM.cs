using System.ComponentModel.DataAnnotations;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.ViewModels;

public class FriendRequestVM
{
    [StringLength(250)]
    [Required(ErrorMessage = ValidationMessages.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationMessages.ValidEmail)]
    [RegularExpression(ValidationRegex.EmailRegex, ErrorMessage = ValidationMessages.ValidEmail)]
    public string Email { get; set; } = null!;

    public string Message { get; set; } = "No account was found with {0}. Would you like to refere to {0}.";

    public int Id { get; set; }
    public string ProfileImagePath { get; set; }
    public string Name { get; set; }
}
