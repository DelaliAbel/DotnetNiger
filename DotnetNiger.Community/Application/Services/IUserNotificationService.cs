using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Notifications personnelles des utilisateurs.</summary>
public interface IUserNotificationService
{
    /// <summary>Notifications d'un utilisateur, de la plus récente à la plus ancienne.</summary>
    Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId);
    /// <summary>Nombre de notifications non lues.</summary>
    Task<int> GetUnreadCountAsync(Guid userId);
    /// <summary>Envoie une notification à un utilisateur.</summary>
    Task SendNotificationAsync(Guid userId, string message);
    /// <summary>Marque une notification comme lue.</summary>
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
    /// <summary>Marque toutes les notifications comme lues.</summary>
    Task<bool> MarkAllAsReadAsync(Guid userId);
}
