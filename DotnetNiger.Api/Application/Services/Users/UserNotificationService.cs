using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Users;

/// <summary>Service de gestion des notifications utilisateur.</summary>
public class UserNotificationService : IUserNotificationService
{
    private readonly DotnetNigerDbContext _db;

    public UserNotificationService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère toutes les notifications d'un utilisateur.</summary>
    public async Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<Notification>().AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync(ct);
    }

    /// <summary>Retourne le nombre de notifications non lues.</summary>
    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<Notification>().CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    /// <summary>Crée et envoie une notification à un utilisateur.</summary>
    public async Task SendNotificationAsync(Guid userId, string message, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _db.Set<Notification>().Add(notification);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Marque une notification comme lue.</summary>
    public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);
        if (notification == null) return false;
        notification.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Marque toutes les notifications comme lues.</summary>
    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _db.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), ct);
    }
}
