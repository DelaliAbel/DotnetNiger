namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Réponse après une demande d'oubli de données.</summary>
public record ForgetMeResponse(
    string Message,
    DateTime CompletedAt);
