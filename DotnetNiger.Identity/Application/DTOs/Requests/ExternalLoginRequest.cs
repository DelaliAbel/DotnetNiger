using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de connexion externe (OAuth).</summary>
public record ExternalLoginRequest(
    [Required] string Provider,
    string? ReturnUrl);
