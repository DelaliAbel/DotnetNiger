using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Service de modération des événements (publication, approbation, rejet).</summary>
public class EventModerationService : IEventModerationService
{
    private readonly DotnetNigerDbContext _db;

    public EventModerationService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Publie un événement (passe le statut à Published).</summary>
    public async Task<EventResponse?> PublishAsync(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return null;
        ev.Status = EventStatus.Published;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(ev);
    }

    /// <summary>Retire un événement de publication (passe le statut à Draft).</summary>
    public async Task<EventResponse?> UnpublishAsync(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return null;
        ev.Status = EventStatus.Draft;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(ev);
    }

    /// <summary>Approuve un événement en attente de modération.</summary>
    public async Task<EventResponse?> ApproveAsync(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return null;
        ev.Status = EventStatus.Published;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(ev);
    }

    /// <summary>Rejette un événement avec une raison.</summary>
    public async Task<EventResponse?> RejectAsync(Guid id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(Messages.Certificate.RejectReasonRequired);

        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return null;
        ev.Status = EventStatus.Cancelled;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(ev);
    }

    private static EventResponse MapToResponse(Event e) =>
        new(e.Id, e.Title, e.Slug, e.Description, e.StartDate, e.EndDate,
            e.Location, e.CoverImageUrl, e.OrganizerId, e.Status.ToString(),
            e.Status == EventStatus.Published,
            e.CreatedAt, e.UpdatedAt);
}
