namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Résultat d'un signalement de support.</summary>
public class SupportReportResult
{
    /// <summary>Indique si l'envoi a réussi.</summary>
    public bool Success { get; init; }
    /// <summary>Message d'erreur éventuel.</summary>
    public string? Error { get; init; }
}
