using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de changement de mot de passe.</summary>
public record ChangePasswordRequest(
    // <summary>Mot de passe actuel.</summary>
    [Required] string CurrentPassword,
    // <summary>Nouveau mot de passe.</summary>
    [Required] string NewPassword);
