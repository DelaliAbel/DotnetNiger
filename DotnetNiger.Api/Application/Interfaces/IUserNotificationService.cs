using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de notifications utilisateur.</summary>
public interface IUserNotificationService
{
    /// <summary>Récupère les notifications d'un utilisateur.</summary>
    Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId);
    /// <summary>Retourne le nombre de notifications non lues.</summary>
    Task<int> GetUnreadCountAsync(Guid userId);
    /// <summary>Envoie une notification à un utilisateur.</summary>
    Task SendNotificationAsync(Guid userId, string message);
    /// <summary>Marque une notification comme lue.</summary>
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
    /// <summary>Marque toutes les notifications comme lues.</summary>
    Task MarkAllAsReadAsync(Guid userId);
}
