using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de consentement utilisateur (RGPD).</summary>
public record ConsentRequest(
    // <summary>Type de consentement (ex: cookies, newsletter).</summary>
    [Required] string ConsentType,
    // <summary>Version des conditions de consentement.</summary>
    [Required] string ConsentVersion,
    // <summary>Indique si le consentement est accordé.</summary>
    bool Granted = true);
