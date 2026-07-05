using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de changement d'adresse email.</summary>
public record ChangeEmailRequest(
    [Required][EmailAddress] string NewEmail);
