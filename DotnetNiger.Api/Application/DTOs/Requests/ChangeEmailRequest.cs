using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de changement d'adresse e-mail.</summary>
public record ChangeEmailRequest(
    // <summary>Nouvelle adresse e-mail.</summary>
    [Required][EmailAddress] string NewEmail);
