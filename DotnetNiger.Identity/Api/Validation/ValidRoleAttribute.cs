using System.ComponentModel.DataAnnotations;
using DotnetNiger.Common.Constants;

namespace DotnetNiger.Identity.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ValidRoleAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string role && RoleConstants.IsValid(role))
            return ValidationResult.Success;

        return new ValidationResult($"Le rôle '{value}' n'est pas valide. Rôles acceptés : {string.Join(", ", RoleConstants.All)}");
    }
}
