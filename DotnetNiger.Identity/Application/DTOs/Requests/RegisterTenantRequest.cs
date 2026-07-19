using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête d'inscription d'un tenant.</summary>
public record RegisterTenantRequest(
    [Required][StringLength(100, MinimumLength = 2)] string CompanyName,
    [Required][StringLength(50, MinimumLength = 2)][RegularExpression(@"^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Le slug doit contenir uniquement des lettres minuscules, chiffres et tirets")] string Slug,
    [Required][EmailAddress] string AdminEmail,
    [Required][StringLength(100, MinimumLength = 8)] string AdminPassword,
    [Required][StringLength(50, MinimumLength = 1)] string AdminFirstName,
    [Required][StringLength(50, MinimumLength = 1)] string AdminLastName);
