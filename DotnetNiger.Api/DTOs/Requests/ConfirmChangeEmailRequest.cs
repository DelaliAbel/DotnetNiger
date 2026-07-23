using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de confirmation de changement d'adresse e-mail.</summary>
public record ConfirmChangeEmailRequest(
    // <summary>Nouvelle adresse e-mail.</summary>
    [Required][EmailAddress] string NewEmail,
    // <summary>Code de confirmation.</summary>
    [Required] string Code);
