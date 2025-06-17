using System.ComponentModel.DataAnnotations;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.ViewModels;

public class PasswordResetVM
{
    public string ResetToken { get; set; }

    [Required(ErrorMessage = ValidationMessages.CurrentPasswordRequired)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.NewPasswordRequired)]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = ValidationMessages.NewPasswordLength)]
    [RegularExpression(ValidationRegex.PasswordRegex, ErrorMessage = ValidationMessages.ValidNewPassword)]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.ConfirmPasswordRequired)]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = ValidationMessages.MatchPassword)]
    [MinLength(8, ErrorMessage = ValidationMessages.ConfirmPasswordLength)]
    [RegularExpression(ValidationRegex.PasswordRegex, ErrorMessage = ValidationMessages.ValidConfirmPassword)]
    public string ConfirmPassword { get; set; } = null!;
}
