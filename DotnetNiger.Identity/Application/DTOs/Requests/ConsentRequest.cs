using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de consentement utilisateur.</summary>
public record ConsentRequest(
    [Required] string ConsentType,
    [Required] string ConsentVersion,
    bool Granted = true);
