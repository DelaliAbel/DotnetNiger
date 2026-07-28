using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Events;

/// <summary>Service de gestion des inscriptions aux événements.</summary>
public class EventRegistrationService : IEventRegistrationService
{
    private readonly DotnetNigerDbContext _db;

    public EventRegistrationService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Inscrit un utilisateur à un événement publié.</summary>
    public async Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string? avatarUrl)
    {
        var ev = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Status == EventStatus.Published);
        if (ev == null) return null;

        var existing = await _db.EventRegistrations
            .AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        if (existing) return null;

        var affected = await _db.Events
            .Where(e => e.Id == eventId
                && e.Status == EventStatus.Published
                && (e.Capacity == 0 || e.RegisteredCount < e.Capacity))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.RegisteredCount, e => e.RegisteredCount + 1)
                .SetProperty(e => e.UpdatedAt, DateTime.UtcNow));

        if (affected == 0) return null;

        var registration = new EventRegistration
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            RegistrationStatus = "Confirmed",
            RegisteredAt = DateTime.UtcNow
        };
        _db.EventRegistrations.Add(registration);
        await _db.SaveChangesAsync();

        return new EventRegistrationResponse
        {
            Id = registration.Id,
            EventId = eventId,
            EventTitle = ev.Title,
            UserId = userId,
            UserName = userName,
            AvatarUrl = avatarUrl ?? "",
            RegisteredAt = registration.RegisteredAt,
            RegistrationStatus = "Confirmed"
        };
    }

    /// <summary>Annule l'inscription d'un utilisateur à un événement.</summary>
    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        var registration = await _db.EventRegistrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
        if (registration == null) return false;

        _db.EventRegistrations.Remove(registration);
        await _db.SaveChangesAsync();

        await _db.Events
            .Where(e => e.Id == eventId && e.RegisteredCount > 0)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.RegisteredCount, e => e.RegisteredCount - 1)
                .SetProperty(e => e.UpdatedAt, DateTime.UtcNow));

        return true;
    }
}
