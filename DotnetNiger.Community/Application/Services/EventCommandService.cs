using DotnetNiger.Common.Extensions;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Notifications;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Commandes de modification des événements.</summary>
public class EventCommandService(AppDbContext db, IServiceScopeFactory scopeFactory, ILogger<EventCommandService> logger) : IEventCommandService
{
    /// <inheritdoc/>
    public async Task<EventResponse> CreateAsync(CreateEventRequest request, Guid userId)
    {
        var ev = BuildEventEntity(request, userId);

        AddEventMedia(ev, request);
        AddEventSpeakers(ev, request);
        await AssignTags(ev, request.TagNames);

        db.Events.Add(ev);
        await db.SaveChangesAsync();

        SendNewEventNotification(ev);

        return EventMappers.ToResponse(ev);
    }

    private static Event BuildEventEntity(CreateEventRequest request, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        Slug = SlugGenerator.GenerateSlug(request.Title),
        Description = request.Description,
        Location = request.Location,
        EventType = request.EventType,
        Category = request.Category,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        CoverImageUrl = request.CoverImageUrl,
        CreatedBy = userId,
        Capacity = request.Capacity,
        MeetupLink = request.MeetupLink,
        IsPublished = request.IsPublished,
        IsArchived = request.IsArchived,
        SubmittedAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static void AddEventMedia(Event ev, CreateEventRequest request)
    {
        foreach (var imageUrl in request.GalleryImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            ev.Medias.Add(new EventMedia { Id = Guid.NewGuid(), EventId = ev.Id, Type = "Image", Url = imageUrl });
    }

    private static void AddEventSpeakers(Event ev, CreateEventRequest request)
    {
        foreach (var speaker in request.Speakers)
            ev.Speakers.Add(new Speaker { Id = Guid.NewGuid(), EventId = ev.Id, UserId = speaker.UserId, Name = speaker.Name, Role = speaker.Role, AvatarUrl = speaker.AvatarUrl });
    }

    private void SendNewEventNotification(Event ev)
    {
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var notification = scope.ServiceProvider.GetRequiredService<INotificationService>();
            try { await notification.NotifyNewEventAsync(ev.Title, ev.Description, ev.StartDate); }
            catch (Exception ex) { logger.LogWarning(ex, "Échec de notification pour le nouvel événement {Title}", ev.Title); }
        });
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> UpdateAsync(Guid id, CreateEventRequest request, Guid userId, bool isAdmin)
    {
        var ev = await db.Events.Include(e => e.Medias).Include(e => e.EventTags).Include(e => e.Speakers).FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        if (ev.CreatedBy != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Event.NotAuthorizedModify);

        ev.Title = request.Title;
        ev.Slug = SlugGenerator.GenerateSlug(request.Title);
        ev.Description = request.Description;
        ev.Location = request.Location;
        ev.EventType = request.EventType;
        ev.Category = request.Category;
        ev.StartDate = request.StartDate;
        ev.EndDate = request.EndDate;
        ev.CoverImageUrl = request.CoverImageUrl;
        ev.Capacity = request.Capacity;
        ev.MeetupLink = request.MeetupLink;
        ev.IsPublished = request.IsPublished;
        ev.IsArchived = request.IsArchived;
        ev.UpdatedAt = DateTime.UtcNow;

        db.EventTags.RemoveRange(ev.EventTags);
        await AssignTags(ev, request.TagNames);

        db.EventMedias.RemoveRange(ev.Medias.Where(m => m.Type == "Image"));
        foreach (var imageUrl in request.GalleryImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            ev.Medias.Add(new EventMedia { Id = Guid.NewGuid(), EventId = ev.Id, Type = "Image", Url = imageUrl });

        db.Speakers.RemoveRange(ev.Speakers);
        foreach (var speaker in request.Speakers)
            ev.Speakers.Add(new Speaker { Id = Guid.NewGuid(), EventId = ev.Id, UserId = speaker.UserId, Name = speaker.Name, Role = speaker.Role, AvatarUrl = speaker.AvatarUrl });

        await db.SaveChangesAsync();
        return EventMappers.ToResponse(ev);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var ev = await db.Events.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return false;
        if (ev.CreatedBy != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Event.NotAuthorizedDelete);
        ev.IsDeleted = true;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task AssignTags(Event ev, List<string> tagNames)
    {
        var names = tagNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count == 0) return;

        var slugs = names.Select(SlugGenerator.GenerateSlug).ToHashSet();
        var existingTags = await db.Tags.Where(t => slugs.Contains(t.Slug)).ToListAsync();
        var existingBySlug = existingTags.ToDictionary(t => t.Slug);

        foreach (var name in names)
        {
            var slug = SlugGenerator.GenerateSlug(name);
            if (!existingBySlug.TryGetValue(slug, out var tag))
            {
                tag = new Tag { Id = Guid.NewGuid(), Name = name, Slug = slug };
                db.Tags.Add(tag);
                existingBySlug[slug] = tag;
            }
            ev.EventTags.Add(new EventTag { EventId = ev.Id, TagId = tag.Id });
        }
    }
}
