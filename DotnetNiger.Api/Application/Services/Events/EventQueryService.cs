using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Events;

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
        var q = _db.Events
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .Include(e => e.Speakers)
            .Include(e => e.Medias)
            .AsNoTracking();

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
        var ev = await _db.Events.AsNoTracking()
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .Include(e => e.Speakers)
            .Include(e => e.Medias)
            .FirstOrDefaultAsync(e => e.Id == id);
        return ev == null ? null : MapToResponse(ev);
    }

    /// <summary>Récupère un événement par son slug.</summary>
    public async Task<EventResponse?> GetBySlugAsync(string slug)
    {
        var ev = await _db.Events.AsNoTracking()
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .Include(e => e.Speakers)
            .Include(e => e.Medias)
            .FirstOrDefaultAsync(e => e.Slug == slug);
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
            .Include(r => r.Event)
            .Where(r => r.EventId == eventId)
            .Select(r => new EventRegistrationResponse
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event!.Title,
                UserId = r.UserId,
                UserName = r.UserName,
                AvatarUrl = r.AvatarUrl,
                RegisteredAt = r.RegisteredAt,
                IsAttended = r.IsAttended,
                RegistrationStatus = r.RegistrationStatus
            })
            .ToListAsync();
    }

    private static EventResponse MapToResponse(Event e)
    {
        var tags = e.EventTags?.Select(et => new TagResponse
        {
            Id = et.Tag.Id, Name = et.Tag.Name, Slug = et.Tag.Slug, UsageCount = et.Tag.UsageCount
        }).ToList() ?? [];

        var speakers = e.Speakers?.Select(s => new SpeakerResponse(
            s.UserId, s.Name, s.Role, s.AvatarUrl)).ToList() ?? [];

        var medias = e.Medias?.Select(m => new EventMediaResponse(
            m.Id, m.Type, m.FileUrl, m.Url, m.Title)).ToList() ?? [];

        var galleryUrls = e.Medias?
            .Where(m => m.Type == "image" && !string.IsNullOrEmpty(m.Url))
            .Select(m => m.Url)
            .ToList() ?? [];

        return new EventResponse(
            e.Id, e.Title, e.Slug, e.Description,
            e.StartDate, e.EndDate, e.Location,
            e.CoverImageUrl, e.CreatedBy,
            e.Status.ToString(), e.Status == EventStatus.Published,
            e.CreatedAt, e.UpdatedAt,
            e.EventType, e.Category, e.OrganizerName,
            e.Capacity, e.RegisteredCount, e.MeetupLink,
            e.RejectionReason, e.SubmittedAt, e.PublishedAt,
            medias, galleryUrls, tags, speakers);
    }
}
