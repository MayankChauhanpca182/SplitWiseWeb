using System.ComponentModel.DataAnnotations;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.ViewModels;

public class LoginVM
{
    [StringLength(250)]
    [Required(ErrorMessage = ValidationMessages.EmailRequired)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.PasswordRequired)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool IsRememberMe { get; set; }
}
