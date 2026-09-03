using System.Threading;
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
    public async Task<EventResponse> CreateAsync(CreateEventRequest request, Guid organizerId, bool isAdmin, bool isCollaborator, CancellationToken ct = default)
    {
        var slug = await GenerateUniqueSlug(request.Slug, request.Title, ct);

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
            Status = request.IsArchived ? EventStatus.Archived :
                     request.IsPublished ? EventStatus.Published : EventStatus.Draft,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null
        };

        await SyncEventTagsAsync(eventEntity, request.TagNames, request.TagIds, ct);
        SyncEventSpeakers(eventEntity.Id, request.Speakers);
        SyncEventMedias(eventEntity.Id, request.GalleryImageUrls);

        _db.Events.Add(eventEntity);
        await _db.SaveChangesAsync(ct);
        return MapToResponse(eventEntity);
    }

    /// <summary>Met à jour un événement existant.</summary>
    public async Task<EventResponse?> UpdateAsync(Guid id, UpdateEventRequest request, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var eventEntity = await _db.Events
            .Include(e => e.EventTags)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (eventEntity == null) return null;

        if (!isAdmin && eventEntity.OrganizerId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cet événement.");

        if (request.Title != null) eventEntity.Title = request.Title;
        if (request.Slug != null) eventEntity.Slug = await EnsureUniqueSlug(request.Slug, eventEntity.Id, ct);
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
        if (request.IsArchived.HasValue)
            eventEntity.Status = request.IsArchived.Value ? EventStatus.Archived : EventStatus.Draft;
        else if (request.IsPublished.HasValue)
        {
            eventEntity.Status = request.IsPublished.Value ? EventStatus.Published : EventStatus.Unpublished;
            if (request.IsPublished.Value)
                eventEntity.PublishedAt = DateTime.UtcNow;
        }

        if (request.TagNames != null)
            await SyncEventTagsAsync(eventEntity, request.TagNames, null, ct);

        SyncEventSpeakers(eventEntity.Id, request.Speakers);
        SyncEventMedias(eventEntity.Id, request.GalleryImageUrls);

        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return MapToResponse(eventEntity);
    }

    /// <summary>Supprime un événement (suppression définitive).</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var eventEntity = await _db.Events
            .Include(e => e.Registrations)
            .Include(e => e.Comments)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (eventEntity == null) return false;
        if (!isAdmin && eventEntity.OrganizerId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cet événement.");

        if (eventEntity.Registrations.Count != 0)
            _db.Set<EventRegistration>().RemoveRange(eventEntity.Registrations);
        if (eventEntity.Comments.Count != 0)
            _db.Set<Comment>().RemoveRange(eventEntity.Comments);

        _db.Events.Remove(eventEntity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Soumet un événement pour modération.</summary>
    public async Task SubmitForReviewAsync(Guid id, CancellationToken ct = default)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new KeyNotFoundException("Événement non trouvé");
        eventEntity.Status = EventStatus.PendingReview;
        eventEntity.SubmittedAt = DateTime.UtcNow;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Publie un événement.</summary>
    public async Task PublishAsync(Guid id, CancellationToken ct = default)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new KeyNotFoundException("Événement non trouvé");
        eventEntity.Status = EventStatus.Published;
        eventEntity.PublishedAt = DateTime.UtcNow;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Annule un événement.</summary>
    public async Task CancelAsync(Guid id, CancellationToken ct = default)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new KeyNotFoundException("Événement non trouvé");
        eventEntity.Status = EventStatus.Cancelled;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task SyncEventTagsAsync(Event eventEntity, List<string>? tagNames, List<Guid>? tagIds, CancellationToken ct = default)
    {
        if (eventEntity.EventTags.Count != 0)
        {
            _db.Set<EventTag>().RemoveRange(eventEntity.EventTags);
            eventEntity.EventTags.Clear();
        }

        var tagsToLink = new List<Tag>();

        if (tagIds?.Count > 0)
        {
            var existing = await _db.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync(ct);
            tagsToLink.AddRange(existing);
        }

        if (tagNames?.Count > 0)
        {
            var existingNames = await _db.Tags.Where(t => tagNames.Contains(t.Name)).ToListAsync(ct);
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

    private void SyncEventMedias(Guid eventId, List<string>? galleryImageUrls)
    {
        var existing = _db.EventMedias.Where(m => m.EventId == eventId).ToList();
        if (existing.Count != 0)
            _db.EventMedias.RemoveRange(existing);

        if (galleryImageUrls is null) return;

        foreach (var url in galleryImageUrls)
        {
            if (string.IsNullOrWhiteSpace(url)) continue;
            _db.EventMedias.Add(new EventMedia
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Type = "image",
                FileUrl = url,
                FileType = "image",
                Url = url,
                Title = string.Empty
            });
        }
    }

    private async Task<string> GenerateUniqueSlug(string? providedSlug, string title, CancellationToken ct = default)
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
        while (await _db.Events.AnyAsync(e => e.Slug == candidate, ct))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }

    private async Task<string> EnsureUniqueSlug(string slug, Guid entityId, CancellationToken ct = default)
    {
        var candidate = slug;
        var suffix = 1;
        while (await _db.Events.AnyAsync(e => e.Slug == candidate && e.Id != entityId, ct))
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
            e.MeetupLink, e.RejectionReason, e.SubmittedAt, e.PublishedAt,
            e.Medias?.Select(m => new EventMediaResponse(m.Id, m.Type, m.FileUrl, m.Url, m.Title)).ToList() ?? [],
            e.Medias?.Where(m => m.Type == "image" && !string.IsNullOrEmpty(m.Url)).Select(m => m.Url).ToList() ?? [],
            [],
            e.Speakers?.Select(s => new SpeakerResponse(s.UserId, s.Name, s.Role, s.AvatarUrl)).ToList() ?? []);
}
