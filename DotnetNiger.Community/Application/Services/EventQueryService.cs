using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Requêtes de consultation des événements.</summary>
public class EventQueryService(AppDbContext db) : IEventQueryService
{
    /// <inheritdoc/>
    public async Task<PaginatedResponse<EventResponse>> GetAllAsync(string? published, string? past, string? eventType, string? query, string? tag, DateTime? startDateFrom, DateTime? startDateTo, Guid? submitterId, int page, int pageSize, Guid? after)
    {
        var q = ApplyFilters(BuildQuery(), published, past, eventType, query, tag, startDateFrom, startDateTo, submitterId);

        List<Event> items;
        int total;

        if (after.HasValue)
        {
            items = await q.Where(e => e.Id > after.Value).OrderBy(e => e.Id).Take(pageSize).ToListAsync();
            total = items.Count;
        }
        else
        {
            total = await q.CountAsync();
            items = await q.OrderByDescending(e => e.StartDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        return new PaginatedResponse<EventResponse> { Items = items.Select(EventMappers.ToResponse).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <inheritdoc/>
    public async Task<List<EventResponse>> GetUpcomingAsync(int page = 1, int pageSize = 10)
    {
        var items = await BuildQuery()
            .Where(e => e.IsPublished && e.EndDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        return items.Select(EventMappers.ToResponse).ToList();
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var ev = await BuildQuery().FirstOrDefaultAsync(e => e.Id == id);
        return ev is null ? null : EventMappers.ToResponse(ev);
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> GetBySlugAsync(string slug)
    {
        var ev = await BuildQuery().FirstOrDefaultAsync(e => e.Slug == slug);
        return ev is null ? null : EventMappers.ToResponse(ev);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page = 1, int pageSize = 10)
    {
        var q = BuildQuery().Where(e => !e.IsPublished && !e.IsDeleted);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(e => e.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedResponse<EventResponse> { Items = items.Select(EventMappers.ToResponse).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <inheritdoc/>
    public async Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId)
    {
        var eventTitle = await db.Events.AsNoTracking().Where(e => e.Id == eventId).Select(e => e.Title).FirstOrDefaultAsync() ?? "";
        return await db.EventRegistrations.AsNoTracking().Where(r => r.EventId == eventId)
            .Select(r => EventMappers.ToRegistrationResponse(r, eventTitle))
            .ToListAsync();
    }

    private IQueryable<Event> BuildQuery() => db.Events.AsNoTracking()
        .Include(e => e.Medias)
        .Include(e => e.EventTags).ThenInclude(et => et.Tag)
        .Include(e => e.Speakers)
        .AsSplitQuery();

    private static IQueryable<Event> ApplyFilters(IQueryable<Event> q, string? published, string? past, string? eventType, string? query, string? tag, DateTime? startDateFrom, DateTime? startDateTo, Guid? submitterId)
    {
        if (published == "true") q = q.Where(e => e.IsPublished);
        else if (published == "false") q = q.Where(e => !e.IsPublished);
        if (past == "true") q = q.Where(e => e.EndDate < DateTime.UtcNow);
        else if (past == "false") q = q.Where(e => e.EndDate >= DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(eventType)) q = q.Where(e => e.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(tag)) q = q.Where(e => e.EventTags.Any(et => et.Tag.Slug == tag));
        if (!string.IsNullOrWhiteSpace(query)) q = q.Where(e => e.Title.Contains(query) || e.Description.Contains(query));
        if (startDateFrom.HasValue) q = q.Where(e => e.StartDate >= startDateFrom.Value);
        if (startDateTo.HasValue) q = q.Where(e => e.StartDate <= startDateTo.Value);
        if (submitterId.HasValue) q = q.Where(e => e.CreatedBy == submitterId.Value);
        return q;
    }
}
