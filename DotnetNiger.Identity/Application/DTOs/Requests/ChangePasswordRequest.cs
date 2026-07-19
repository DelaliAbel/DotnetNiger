using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de changement de mot de passe.</summary>
public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required] string NewPassword);
