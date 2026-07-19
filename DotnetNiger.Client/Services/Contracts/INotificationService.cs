using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface INotificationService
{
    event Action<Guid>? NotificationsChanged;

    Task SendNotificationAsync(Guid userId, string message);
    Task<List<NotificationDto>> GetNotificationsAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid userId, Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
}
