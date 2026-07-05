namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Réponse standardisée de succès avec données typées.</summary>
public class ApiSuccessResponse<T>
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
}
