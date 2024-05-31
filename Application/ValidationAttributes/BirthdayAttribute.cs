using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.ValidationAttributes;

public class BirthdayAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        value is DateTime dateTime && dateTime < DateTime.UtcNow
            ? ValidationResult.Success
            : new ValidationResult("Birth date can't be later than today");
}