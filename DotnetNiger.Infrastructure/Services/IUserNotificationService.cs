using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface IUserNotificationService
{
    Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task SendNotificationAsync(Guid userId, string message);
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
}
