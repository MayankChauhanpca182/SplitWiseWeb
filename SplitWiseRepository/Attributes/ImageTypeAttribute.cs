using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SplitWiseRepository.Attributes;

public class ImageTypeAttribute : ValidationAttribute
{
    private readonly string[] _allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
 
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            string extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedTypes.Contains(extension))
            {
                return new ValidationResult($"Only the following file types are allowed: {string.Join(", ", _allowedTypes)}");
            }
        }
        return ValidationResult.Success!;
    }
}
