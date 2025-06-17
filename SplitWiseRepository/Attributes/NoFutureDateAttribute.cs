using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SplitWiseRepository.Attributes;

public class NoFutureDateAttribute : ValidationAttribute, IClientModelValidator
{
    public NoFutureDateAttribute()
    {
        ErrorMessage = "The date cannot be in the future. Please select a date on or before today.";
    }

    // Server-side validation
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date.Date > DateTime.Today)
            {
                return new ValidationResult(ErrorMessage);
            }
        }
        return ValidationResult.Success;
    }

    // Client-side validation
    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        context.Attributes.Add("data-val", "true");
        context.Attributes.Add("data-val-nofuturedate", ErrorMessage);
        context.Attributes.Add("data-val-nofuturedate-maxdate", DateTime.Today.ToString("yyyy-MM-dd"));
    }
}
