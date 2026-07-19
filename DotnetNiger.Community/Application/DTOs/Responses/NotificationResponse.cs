namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'une notification.</summary>
public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
