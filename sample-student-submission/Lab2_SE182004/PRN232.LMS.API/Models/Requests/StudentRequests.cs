using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace PRN232.LMS.API.Models.Requests;

public class CreateStudentRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [CustomValidation(typeof(StudentValidationRules), nameof(StudentValidationRules.ValidateFptStyleCode))]
    public string StudentCode { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class UpdateStudentRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class PatchStudentRequest
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }
}

public static class StudentValidationRules
{
    public static ValidationResult ValidateFptStyleCode(string? value, ValidationContext context)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               System.Text.RegularExpressions.Regex.IsMatch(value, @"^(SE|CE|IA|AI)\d{5}$")
            ? ValidationResult.Success!
            : new ValidationResult("StudentCode must match FPT style format like SE19886.");
    }
}
