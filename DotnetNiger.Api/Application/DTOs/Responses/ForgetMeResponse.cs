namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse de demande d'oubli des données (RGPD).</summary>
public record ForgetMeResponse(
    // <summary>Message de confirmation.</summary>
    string Message,
    // <summary>Date et heure de finalisation de la suppression.</summary>
    DateTime CompletedAt);
