namespace DotnetNiger.Gateway.Application.DTOs.Responses;

/// <summary>
/// DTO pour les réponses d'erreur
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
