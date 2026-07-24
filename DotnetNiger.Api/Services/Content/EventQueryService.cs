using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Service de consultation des événements (requêtes en lecture seule).</summary>
public class EventQueryService : IEventQueryService
{
    private readonly DotnetNigerDbContext _db;

    public EventQueryService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère la liste paginée des événements avec filtres.</summary>
    public async Task<PaginatedResponse<EventResponse>> GetAllAsync(
        string? status, string? query, string? location,
        string? category, string? tag, DateTime? from, DateTime? to,
        Guid? organizerId, int page, int pageSize, Guid? createdBy = null)
    {
        var q = _db.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<EventStatus>(status, true, out var es))
                q = q.Where(e => e.Status == es);
        }
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(e => e.Title.Contains(query) || (e.Description != null && e.Description.Contains(query)));
        if (!string.IsNullOrWhiteSpace(location))
            q = q.Where(e => e.Location != null && e.Location.Contains(location));
        if (from.HasValue) q = q.Where(e => e.StartDate >= from.Value);
        if (to.HasValue) q = q.Where(e => e.EndDate <= to.Value);
        if (organizerId.HasValue) q = q.Where(e => e.OrganizerId == organizerId.Value);
        if (createdBy.HasValue) q = q.Where(e => e.CreatedBy == createdBy.Value);

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<EventResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Récupère un événement par identifiant.</summary>
    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        return ev == null ? null : MapToResponse(ev);
    }

    /// <summary>Récupère les événements en attente de modération.</summary>
    public async Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page, int pageSize)
    {
        return await GetAllAsync("PendingReview", null, null, null, null, null, null, null, page, pageSize);
    }

    /// <summary>Récupère les inscriptions d'un événement.</summary>
    public async Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId)
    {
        return await _db.EventRegistrations.AsNoTracking()
            .Where(r => r.EventId == eventId)
            .Select(r => new EventRegistrationResponse
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event!.Title,
                UserId = r.UserId,
                UserName = "",
                AvatarUrl = "",
                RegisteredAt = r.RegisteredAt,
                IsAttended = r.IsAttended,
                RegistrationStatus = r.RegistrationStatus
            })
            .ToListAsync();
    }

    private static EventResponse MapToResponse(Event e) =>
        new(e.Id, e.Title, e.Slug, e.Description, e.StartDate, e.EndDate,
            e.Location, e.CoverImageUrl, e.CreatedBy, e.Status.ToString(),
            e.Status == EventStatus.Published,
            e.CreatedAt, e.UpdatedAt);
}
