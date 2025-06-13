using System.ComponentModel.DataAnnotations;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.ViewModels;

public class RegisterUserVM
{
    [Required(ErrorMessage = ValidationMessages.FirstNameRequired)]
    [StringLength(50, ErrorMessage = ValidationMessages.FirstNameLength)]
    [RegularExpression(ValidationRegex.NameRegex, ErrorMessage = ValidationMessages.ValidFirstName)]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.LastNameRequired)]
    [StringLength(50, ErrorMessage = ValidationMessages.LastNameLength)]
    [RegularExpression(ValidationRegex.NameRegex, ErrorMessage = ValidationMessages.ValidLastName)]
    public string LastName { get; set; } = null!;

    [StringLength(250)]
    [Required(ErrorMessage = ValidationMessages.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationMessages.ValidEmail)]
    [RegularExpression(ValidationRegex.EmailRegex,
        ErrorMessage = ValidationMessages.ValidEmail)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.PasswordRequired)]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = ValidationMessages.PasswordLength)]
    [RegularExpression(ValidationRegex.PasswordRegex,
        ErrorMessage = ValidationMessages.ValidPassword)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.ConfirmPasswordRequired)]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = ValidationMessages.MatchPassword)]
    [MinLength(8, ErrorMessage = ValidationMessages.ConfirmPasswordLength)]
    [RegularExpression(ValidationRegex.PasswordRegex,
        ErrorMessage = ValidationMessages.ValidConfirmPassword)]
    public string ConfirmPassword { get; set; } = null!;
}
