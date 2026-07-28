namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'erreur générique de l'API.</summary>
public record ErrorResponse(
    // <summary>Message d'erreur principal.</summary>
    string Message,
    // <summary>Code d'erreur technique.</summary>
    string? Code = null,
    // <summary>Liste détaillée des erreurs de validation.</summary>
    IList<string>? Errors = null);
