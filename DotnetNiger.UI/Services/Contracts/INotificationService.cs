using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface INotificationService
{
    event Action<Guid>? NotificationsChanged;

    Task SendNotificationAsync(Guid userId, string message, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetNotificationsAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
