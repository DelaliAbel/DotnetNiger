namespace DotnetNiger.Domain.DTOs.Responses;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
