namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Réponse standardisée pour les erreurs API Identity.</summary>
public record ErrorResponse(
    string Message,
    string? Code = null,
    IList<string>? Errors = null);
