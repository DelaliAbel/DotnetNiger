using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Modération des événements (publication, approbation, rejet).</summary>
public class EventModerationService(AppDbContext db) : IEventModerationService
{
    private IQueryable<Event> Query() => db.Events
        .Include(e => e.Medias)
        .Include(e => e.EventTags).ThenInclude(et => et.Tag)
        .Include(e => e.Speakers)
        .AsSplitQuery();

    /// <inheritdoc/>
    public async Task<EventResponse?> PublishAsync(Guid id)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        ev.IsPublished = true;
        ev.PublishedAt = DateTime.UtcNow;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return EventMappers.ToResponse(ev);
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> UnpublishAsync(Guid id)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        ev.IsPublished = false;
        ev.PublishedAt = null;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return EventMappers.ToResponse(ev);
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> ApproveAsync(Guid id) => await PublishAsync(id);

    /// <inheritdoc/>
    public async Task<EventResponse?> RejectAsync(Guid id, string reason)
    {
        var ev = await Query().FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        ev.IsPublished = false;
        ev.RejectionReason = reason;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return EventMappers.ToResponse(ev);
    }
}
