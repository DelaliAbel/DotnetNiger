using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class UserNotificationService(AppDbContext db) : IUserNotificationService
{
    public async Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId)
    {
        return await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => MapNotification(n))
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

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

    public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notification is null) return false;

        notification.IsRead = true;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var unread = await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unread.Count == 0) return false;

        foreach (var n in unread)
            n.IsRead = true;

        await db.SaveChangesAsync();
        return true;
    }

    private static NotificationResponse MapNotification(Notification n) => new()
    {
        Id = n.Id,
        Message = n.Message,
        CreatedAt = n.CreatedAt,
        IsRead = n.IsRead
    };
}
