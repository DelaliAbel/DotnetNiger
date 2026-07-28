namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse de consentement enregistré.</summary>
public record ConsentResponse(
    // <summary>Type de consentement.</summary>
    string ConsentType,
    // <summary>Version des conditions.</summary>
    string ConsentVersion,
    // <summary>Indique si le consentement a été accordé.</summary>
    bool Granted,
    // <summary>Date d'enregistrement du consentement.</summary>
    DateTime CreatedAt);
