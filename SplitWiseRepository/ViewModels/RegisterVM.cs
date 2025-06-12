using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Attributes;
using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class RegisterVM
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(30, ErrorMessage = "First name cannot exceed 30 characters")]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "First name must contain only letters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(30, ErrorMessage = "Last name cannot exceed 30 characters")]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Last name must contain only letters.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]+$",
        ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Confirm Password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]+$",
        ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
    public string ConfirmPassword { get; set; } = null!;
}
