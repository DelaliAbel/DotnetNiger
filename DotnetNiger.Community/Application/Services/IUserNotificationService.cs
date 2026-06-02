using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

public interface IUserNotificationService
{
    Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task SendNotificationAsync(Guid userId, string message);
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
}
