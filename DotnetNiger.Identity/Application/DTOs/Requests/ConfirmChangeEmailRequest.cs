using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de confirmation de changement d'email.</summary>
public record ConfirmChangeEmailRequest(
    [Required][EmailAddress] string NewEmail,
    [Required] string Code);
