namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Réponse contenant les données de consentement.</summary>
public record ConsentResponse(
    string ConsentType,
    string ConsentVersion,
    bool Granted,
    DateTime CreatedAt);
