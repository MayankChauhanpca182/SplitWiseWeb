using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Attributes;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class UserVM
{
    public int Id { get; set; } = 0;
    public string EmailAddress { get; set; }
    
    [Required(ErrorMessage = ValidationMessages.FirstNameRequired)]
    [StringLength(50, ErrorMessage = ValidationMessages.FirstNameLength)]
    [RegularExpression(ValidationRegex.NameRegex, ErrorMessage = ValidationMessages.ValidFirstName)]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = ValidationMessages.LastNameRequired)]
    [StringLength(50, ErrorMessage = ValidationMessages.LastNameLength)]
    [RegularExpression(ValidationRegex.NameRegex, ErrorMessage = ValidationMessages.ValidLastName)]
    public string LastName { get; set; } = null!;

    [ImageType]
    public IFormFile ProfileImage { get; set; }
    public string ProfileImagePath { get; set; }

    [Required(ErrorMessage = ValidationMessages.CurrencyRequired)]
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.CurrencyRequired)]
    public int CurrencyId { get; set; } = 23;

    [StringLength(500)]
    public string Address { get; set; }

    [Required(ErrorMessage = ValidationMessages.BirthdateRequired)]
    [DataType(DataType.Date)]
    [NoFutureDate(ErrorMessage = ValidationMessages.NoFutureBirthdate)]
    public DateTime? Birthdate { get; set; }

    public List<Currency> Currencies { get; set; } = new List<Currency>();
}
