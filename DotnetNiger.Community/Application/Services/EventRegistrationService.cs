using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des inscriptions aux événements.</summary>
public class EventRegistrationService(AppDbContext db) : IEventRegistrationService
{
    /// <inheritdoc/>
    public async Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string avatarUrl = "")
    {
        var existing = await db.EventRegistrations.AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        if (existing) return null;

        using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var rows = await db.Events.Where(e => e.Id == eventId && e.RegisteredCount < e.Capacity)
                .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.RegisteredCount, e => e.RegisteredCount + 1));

            if (rows == 0) return null;

            var ev = await db.Events.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev is null) return null;

            var registration = new EventRegistration
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                UserName = userName,
                AvatarUrl = avatarUrl,
                RegisteredAt = DateTime.UtcNow,
            };

            db.EventRegistrations.Add(registration);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return EventMappers.ToRegistrationResponse(registration, ev.Title);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        var reg = await db.EventRegistrations.FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
        if (reg is null) return false;

        using var tx = await db.Database.BeginTransactionAsync();
        await db.Events.Where(e => e.Id == eventId && e.RegisteredCount > 0)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.RegisteredCount, e => e.RegisteredCount - 1));

        db.EventRegistrations.Remove(reg);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }
}
