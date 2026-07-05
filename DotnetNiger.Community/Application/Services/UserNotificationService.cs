using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Notifications personnelles des utilisateurs (lecture, marquage comme lu).</summary>
public class UserNotificationService(AppDbContext db) : IUserNotificationService
{
    /// <summary>Notifications d'un utilisateur, de la plus récente à la plus ancienne.</summary>
    public async Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId)
    {
        return await db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => MapNotification(n))
            .ToListAsync();
    }

    /// <summary>Nombre de notifications non lues pour un utilisateur.</summary>
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await db.Notifications.AsNoTracking().CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    /// <summary>Envoie une notification à un utilisateur.</summary>
    public async Task SendNotificationAsync(Guid userId, string message)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }

    /// <summary>Marque une notification spécifique comme lue.</summary>
    public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notification is null) return false;

        notification.IsRead = true;
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Marque toutes les notifications non lues comme lues en une seule requête.</summary>
    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var rows = await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

        return rows > 0;
    }

    private static NotificationResponse MapNotification(Notification n) => new()
    {
        Id = n.Id,
        Message = n.Message,
        CreatedAt = n.CreatedAt,
        IsRead = n.IsRead
    };
}
