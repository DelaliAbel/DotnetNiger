using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Events;

/// <summary>Service de création, modification et suppression des événements.</summary>
public class EventCommandService : IEventCommandService
{
    private readonly DotnetNigerDbContext _db;

    public EventCommandService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Crée un nouvel événement avec ses tags.</summary>
    public async Task<EventResponse> CreateAsync(CreateEventRequest request, Guid organizerId, bool isAdmin, bool isCollaborator)
    {
        var slug = await GenerateUniqueSlug(request.Slug, request.Title);

        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = slug,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            CoverImageUrl = request.CoverImageUrl,
            EventType = request.EventType,
            Category = request.Category,
            OrganizerId = organizerId,
            OrganizerName = request.OrganizerName,
            Capacity = request.Capacity,
            MeetupLink = request.MeetupLink,
            CreatedBy = organizerId,
            IsPublished = request.IsPublished,
            IsArchived = request.IsArchived,
            Status = EventStatus.Draft
        };

        await SyncEventTagsAsync(eventEntity, request.TagNames, request.TagIds);
        SyncEventSpeakers(eventEntity.Id, request.Speakers);

        _db.Events.Add(eventEntity);
        await _db.SaveChangesAsync();
        return MapToResponse(eventEntity);
    }

    /// <summary>Met à jour un événement existant.</summary>
    public async Task<EventResponse?> UpdateAsync(Guid id, UpdateEventRequest request, Guid userId, bool isAdmin)
    {
        var eventEntity = await _db.Events
            .Include(e => e.EventTags)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (eventEntity == null) return null;

        if (!isAdmin && eventEntity.OrganizerId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cet événement.");

        if (request.Title != null) eventEntity.Title = request.Title;
        if (request.Slug != null) eventEntity.Slug = await EnsureUniqueSlug(request.Slug, eventEntity.Id);
        if (request.Description != null) eventEntity.Description = request.Description;
        if (request.Location != null) eventEntity.Location = request.Location;
        if (request.CoverImageUrl != null) eventEntity.CoverImageUrl = request.CoverImageUrl;
        if (request.StartDate.HasValue) eventEntity.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) eventEntity.EndDate = request.EndDate.Value;
        if (request.EventType != null) eventEntity.EventType = request.EventType;
        if (request.Category != null) eventEntity.Category = request.Category;
        if (request.OrganizerName != null) eventEntity.OrganizerName = request.OrganizerName;
        if (request.Capacity.HasValue) eventEntity.Capacity = request.Capacity.Value;
        if (request.MeetupLink != null) eventEntity.MeetupLink = request.MeetupLink;
        if (request.IsPublished.HasValue) eventEntity.IsPublished = request.IsPublished.Value;
        if (request.IsArchived.HasValue) eventEntity.IsArchived = request.IsArchived.Value;

        if (request.TagNames != null)
            await SyncEventTagsAsync(eventEntity, request.TagNames, null);

        SyncEventSpeakers(eventEntity.Id, request.Speakers);

        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(eventEntity);
    }

    /// <summary>Supprime un événement (suppression définitive).</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var eventEntity = await _db.Events
            .Include(e => e.Registrations)
            .Include(e => e.Comments)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (eventEntity == null) return false;
        if (!isAdmin && eventEntity.OrganizerId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cet événement.");

        if (eventEntity.Registrations.Count != 0)
            _db.Set<EventRegistration>().RemoveRange(eventEntity.Registrations);
        if (eventEntity.Comments.Count != 0)
            _db.Set<Comment>().RemoveRange(eventEntity.Comments);

        _db.Events.Remove(eventEntity);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Soumet un événement pour modération.</summary>
    public async Task SubmitForReviewAsync(Guid id)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Événement non trouvé");
        eventEntity.Status = EventStatus.PendingReview;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Publie un événement.</summary>
    public async Task PublishAsync(Guid id)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Événement non trouvé");
        eventEntity.Status = EventStatus.Published;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Annule un événement.</summary>
    public async Task CancelAsync(Guid id)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Événement non trouvé");
        eventEntity.Status = EventStatus.Cancelled;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task SyncEventTagsAsync(Event eventEntity, List<string>? tagNames, List<Guid>? tagIds)
    {
        if (eventEntity.EventTags.Count != 0)
        {
            _db.Set<EventTag>().RemoveRange(eventEntity.EventTags);
            eventEntity.EventTags.Clear();
        }

        var tagsToLink = new List<Tag>();

        if (tagIds?.Count > 0)
        {
            var existing = await _db.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            tagsToLink.AddRange(existing);
        }

        if (tagNames?.Count > 0)
        {
            var existingNames = await _db.Tags.Where(t => tagNames.Contains(t.Name)).ToListAsync();
            var missingNames = tagNames.Except(existingNames.Select(t => t.Name)).ToList();

            foreach (var name in missingNames)
            {
                var tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Slug = name.ToLowerInvariant().Replace(" ", "-")
                };
                _db.Tags.Add(tag);
                tagsToLink.Add(tag);
            }
            tagsToLink.AddRange(existingNames.Where(t => !tagsToLink.Any(x => x.Id == t.Id)));
        }

        foreach (var tag in tagsToLink.DistinctBy(t => t.Id))
        {
            eventEntity.EventTags.Add(new EventTag { EventId = eventEntity.Id, TagId = tag.Id });
        }
    }

    private void SyncEventSpeakers(Guid eventId, List<SpeakerRequest>? speakers)
    {
        var existing = _db.Speakers.Where(s => s.EventId == eventId).ToList();
        if (existing.Count != 0)
            _db.Speakers.RemoveRange(existing);

        if (speakers is null) return;

        foreach (var s in speakers)
        {
            _db.Speakers.Add(new Speaker
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = s.UserId,
                Name = s.Name,
                Role = s.Role,
                AvatarUrl = s.AvatarUrl
            });
        }
    }

    private async Task<string> GenerateUniqueSlug(string? providedSlug, string title)
    {
        var baseSlug = !string.IsNullOrWhiteSpace(providedSlug)
            ? providedSlug
            : title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
                .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
                .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
                .Replace("ç", "c").Replace("œ", "oe").Replace("æ", "ae");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "evenement";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Events.AnyAsync(e => e.Slug == candidate))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }

    private async Task<string> EnsureUniqueSlug(string slug, Guid entityId)
    {
        var candidate = slug;
        var suffix = 1;
        while (await _db.Events.AnyAsync(e => e.Slug == candidate && e.Id != entityId))
        {
            candidate = $"{slug}-{suffix++}";
        }
        return candidate;
    }

    private static EventResponse MapToResponse(Event e) =>
        new(e.Id, e.Title, e.Slug, e.Description, e.StartDate, e.EndDate,
            e.Location, e.CoverImageUrl, e.CreatedBy, e.Status.ToString(),
            e.Status == EventStatus.Published,
            e.CreatedAt, e.UpdatedAt,
            e.EventType, e.Category, e.OrganizerName, e.Capacity, e.RegisteredCount,
            e.MeetupLink, e.RejectionReason, e.SubmittedAt, e.PublishedAt, [], [], [], []);
}
