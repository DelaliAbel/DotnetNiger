using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

public class EventModerationService : IEventModerationService
{
    private readonly DotnetNigerDbContext _db;

    public EventModerationService(DotnetNigerDbContext db) => _db = db;

    public async Task<EventResponse?> PublishAsync(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return null;
        ev.Status = EventStatus.Published;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(ev);
    }

    public async Task<EventResponse?> UnpublishAsync(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return null;
        ev.Status = EventStatus.Draft;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(ev);
    }

    public async Task<EventResponse?> ApproveAsync(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return null;
        ev.Status = EventStatus.Published;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(ev);
    }

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
