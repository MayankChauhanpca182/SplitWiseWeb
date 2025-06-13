using System.ComponentModel.DataAnnotations;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.ViewModels;

public class LoginVM
{
    [StringLength(250)]
    [Required(ErrorMessage = ValidationMessages.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationMessages.ValidEmail)]
    [RegularExpression(ValidationRegex.EmailRegex, ErrorMessage = ValidationMessages.ValidEmail)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.PasswordRequired)]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = ValidationMessages.PasswordLength)]
    public string Password { get; set; } = null!;

    public bool IsRememberMe { get; set; }
}
