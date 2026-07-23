namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse générique de succès de l'API.</summary>
public class ApiSuccessResponse<T>
{
    /// <summary>Indique si l'opération a réussi.</summary>
    public bool Success { get; set; } = true;
    /// <summary>Message descriptif du résultat.</summary>
    public string? Message { get; set; }
    /// <summary>Données retournées par l'opération.</summary>
    public T? Data { get; set; }
}
