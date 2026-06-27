using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Notifications;
using DotnetNiger.Community.Domain;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des événements : CRUD, inscriptions et workflow de modération.</summary>
public class EventService(AppDbContext db, IServiceScopeFactory scopeFactory, ILogger<EventService> logger) : IEventService
{
    /// <summary>Recherche des événements avec filtres multiples. Supporte le curseur (after) ou la pagination classique.</summary>
    public async Task<PaginatedResponse<EventResponse>> GetAllAsync(string? published, string? past, string? eventType, string? query, string? tag, DateTime? startDateFrom, DateTime? startDateTo, Guid? submitterId = null, int page = 1, int pageSize = 10, Guid? after = null)
    {
        var q = db.Events
            .AsNoTracking()
            .Include(e => e.Medias)
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .Include(e => e.Speakers)
            .AsSplitQuery()
            .AsQueryable();

        if (published == "true") q = q.Where(e => e.IsPublished);
        if (published == "false") q = q.Where(e => !e.IsPublished);
        if (past == "true") q = q.Where(e => e.EndDate < DateTime.UtcNow);
        if (past == "false") q = q.Where(e => e.EndDate >= DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(eventType)) q = q.Where(e => e.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(tag))
            q = q.Where(e => e.EventTags.Any(et => et.Tag.Slug == tag));
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(e => e.Title.Contains(query) || e.Description.Contains(query));
        if (startDateFrom.HasValue)
            q = q.Where(e => e.StartDate >= startDateFrom.Value);
        if (startDateTo.HasValue)
            q = q.Where(e => e.StartDate <= startDateTo.Value);
        if (submitterId.HasValue)
            q = q.Where(e => e.CreatedBy == submitterId.Value);

        List<EventResponse> items;
        int total;

        if (after.HasValue)
        {
            items = await q
                .Where(e => e.Id > after.Value)
                .OrderBy(e => e.Id)
                .Take(pageSize)
                .Select(e => new EventResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    Slug = e.Slug,
                    Description = e.Description,
                    Location = e.Location,
                    EventType = e.EventType,
                    Category = e.Category,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    CoverImageUrl = e.CoverImageUrl,
                    CreatedBy = e.CreatedBy,
                    SubmittedBy = e.CreatedBy,
                    OrganizerName = e.OrganizerName,
                    Capacity = e.Capacity,
                    RegisteredCount = e.RegisteredCount,
                    IsPublished = e.IsPublished,
                    IsArchived = e.IsArchived,
                    MeetupLink = e.MeetupLink,
                    RejectionReason = e.RejectionReason,
                    SubmittedAt = e.SubmittedAt,
                    PublishedAt = e.PublishedAt,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    Medias = e.Medias.Select(m => new EventMediaResponse
                    {
                        Id = m.Id,
                        Type = m.Type,
                        Url = m.Url,
                        Title = m.Title
                    }).ToList(),
                    GalleryImageUrls = e.Medias.Where(m => m.Type == "Image").Select(m => m.Url).ToList(),
                    Speakers = e.Speakers.Select(s => new SpeakerResponse
                    {
                        Id = s.Id,
                        UserId = s.UserId,
                        Name = s.Name,
                        Role = s.Role,
                        AvatarUrl = s.AvatarUrl
                    }).ToList(),
                    Tags = e.EventTags.Select(et => new TagResponse
                    {
                        Id = et.Tag.Id,
                        Name = et.Tag.Name,
                        Slug = et.Tag.Slug,
                        UsageCount = et.Tag.UsageCount
                    }).ToList()
                })
                .ToListAsync();
            total = items.Count;
        }
        else
        {
            total = await q.CountAsync();
            items = await q
                .OrderByDescending(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    Slug = e.Slug,
                    Description = e.Description,
                    Location = e.Location,
                    EventType = e.EventType,
                    Category = e.Category,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    CoverImageUrl = e.CoverImageUrl,
                    CreatedBy = e.CreatedBy,
                    SubmittedBy = e.CreatedBy,
                    OrganizerName = e.OrganizerName,
                    Capacity = e.Capacity,
                    RegisteredCount = e.RegisteredCount,
                    IsPublished = e.IsPublished,
                    IsArchived = e.IsArchived,
                    MeetupLink = e.MeetupLink,
                    RejectionReason = e.RejectionReason,
                    SubmittedAt = e.SubmittedAt,
                    PublishedAt = e.PublishedAt,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    Medias = e.Medias.Select(m => new EventMediaResponse
                    {
                        Id = m.Id,
                        Type = m.Type,
                        Url = m.Url,
                        Title = m.Title
                    }).ToList(),
                    GalleryImageUrls = e.Medias.Where(m => m.Type == "Image").Select(m => m.Url).ToList(),
                    Speakers = e.Speakers.Select(s => new SpeakerResponse
                    {
                        Id = s.Id,
                        UserId = s.UserId,
                        Name = s.Name,
                        Role = s.Role,
                        AvatarUrl = s.AvatarUrl
                    }).ToList(),
                    Tags = e.EventTags.Select(rt => new TagResponse
                    {
                        Id = rt.Tag.Id,
                        Name = rt.Tag.Name,
                        Slug = rt.Tag.Slug,
                        UsageCount = rt.Tag.UsageCount
                    }).ToList()
                })
                .ToListAsync();
        }

        return new PaginatedResponse<EventResponse> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <summary>Événements publiés à venir, triés par date de début croissante.</summary>
    public async Task<List<EventResponse>> GetUpcomingAsync(int page = 1, int pageSize = 10)
    {
        return await db.Events
            .AsNoTracking()
            .Include(e => e.Medias)
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .Include(e => e.Speakers)
            .AsSplitQuery()
            .Where(e => e.IsPublished && e.EndDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Slug = e.Slug,
                Description = e.Description,
                Location = e.Location,
                EventType = e.EventType,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                CoverImageUrl = e.CoverImageUrl,
                CreatedBy = e.CreatedBy,
                SubmittedBy = e.CreatedBy,
                OrganizerName = e.OrganizerName,
                Capacity = e.Capacity,
                RegisteredCount = e.RegisteredCount,
                IsPublished = e.IsPublished,
                IsArchived = e.IsArchived,
                MeetupLink = e.MeetupLink,
                RejectionReason = e.RejectionReason,
                SubmittedAt = e.SubmittedAt,
                PublishedAt = e.PublishedAt,
                CreatedAt = e.CreatedAt,
                Medias = e.Medias.Select(m => new EventMediaResponse
                {
                    Id = m.Id,
                    Type = m.Type,
                    Url = m.Url,
                    Title = m.Title
                }).ToList(),
                Tags = e.EventTags.Select(et => new TagResponse
                {
                    Id = et.Tag.Id,
                    Name = et.Tag.Name,
                    Slug = et.Tag.Slug,
                    UsageCount = et.Tag.UsageCount
                }).ToList()
            })
            .ToListAsync();
    }

    /// <summary>Détail d'un événement avec médias, speakers et tags.</summary>
    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var ev = await db.Events
            .AsNoTracking()
            .Include(e => e.Medias)
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .Include(e => e.Speakers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);
        return ev is null ? null : MapEvent(ev);
    }

    /// <summary>Détail d'un événement par son slug.</summary>
    public async Task<EventResponse?> GetBySlugAsync(string slug)
    {
        var ev = await db.Events
            .AsNoTracking()
            .Include(e => e.Medias)
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .Include(e => e.Speakers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Slug == slug);
        return ev is null ? null : MapEvent(ev);
    }

    /// <summary>Crée un événement avec ses médias, speakers et tags, puis notifie les abonnés en arrière-plan.</summary>
    public async Task<EventResponse> CreateAsync(CreateEventRequest request, Guid userId)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
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

        foreach (var imageUrl in request.GalleryImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            ev.Medias.Add(new EventMedia
            {
                Id = Guid.NewGuid(),
                EventId = ev.Id,
                Type = "Image",
                Url = imageUrl
            });
        }

        foreach (var speaker in request.Speakers)
        {
            ev.Speakers.Add(new Speaker
            {
                Id = Guid.NewGuid(),
                EventId = ev.Id,
                UserId = speaker.UserId,
                Name = speaker.Name,
                Role = speaker.Role,
                AvatarUrl = speaker.AvatarUrl
            });
        }

        await AssignTags(ev, request.TagNames);
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var notification = scope.ServiceProvider.GetRequiredService<INotificationService>();
            try { await notification.NotifyNewEventAsync(ev.Title, ev.Description, ev.StartDate); }
            catch (Exception ex) { logger.LogWarning(ex, "Échec de notification pour le nouvel événement {Title}", ev.Title); }
        });
        return MapEvent(ev);
    }

    /// <summary>Modifie un événement. Reconstruit les médias, speakers et tags à partir de la requête.</summary>
    public async Task<EventResponse?> UpdateAsync(Guid id, CreateEventRequest request, Guid userId, bool isAdmin)
    {
        var ev = await db.Events
            .Include(e => e.Medias)
            .Include(e => e.EventTags)
            .Include(e => e.Speakers)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        if (ev.CreatedBy != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Event.NotAuthorizedModify);

        ev.Title = request.Title;
        ev.Slug = GenerateSlug(request.Title);
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
        {
            ev.Medias.Add(new EventMedia
            {
                Id = Guid.NewGuid(),
                EventId = ev.Id,
                Type = "Image",
                Url = imageUrl
            });
        }

        db.Speakers.RemoveRange(ev.Speakers);
        foreach (var speaker in request.Speakers)
        {
            ev.Speakers.Add(new Speaker
            {
                Id = Guid.NewGuid(),
                EventId = ev.Id,
                UserId = speaker.UserId,
                Name = speaker.Name,
                Role = speaker.Role,
                AvatarUrl = speaker.AvatarUrl
            });
        }

        await db.SaveChangesAsync();
        return MapEvent(ev);
    }

    /// <summary>Suppression logique : masque l'événement au lieu de le supprimer définitivement.</summary>
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

    /// <summary>Publie un événement et enregistre la date de publication.</summary>
    public async Task<EventResponse?> PublishAsync(Guid id)
    {
        var ev = await db.Events
            .Include(e => e.Medias)
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        ev.IsPublished = true;
        ev.PublishedAt = DateTime.UtcNow;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapEvent(ev);
    }

    /// <summary>Dépublie un événement et efface sa date de publication.</summary>
    public async Task<EventResponse?> UnpublishAsync(Guid id)
    {
        var ev = await db.Events
            .Include(e => e.Medias)
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        ev.IsPublished = false;
        ev.PublishedAt = null;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapEvent(ev);
    }

    /// <summary>Événements soumis en attente de publication (non publiés et non supprimés).</summary>
    public async Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page = 1, int pageSize = 10)
    {
        var query = db.Events
            .AsNoTracking()
            .Where(e => !e.IsPublished && !e.IsDeleted);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Slug = e.Slug,
                Description = e.Description,
                Location = e.Location,
                EventType = e.EventType,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                CoverImageUrl = e.CoverImageUrl,
                CreatedBy = e.CreatedBy,
                SubmittedBy = e.CreatedBy,
                OrganizerName = e.OrganizerName,
                Capacity = e.Capacity,
                RegisteredCount = e.RegisteredCount,
                IsPublished = e.IsPublished,
                IsArchived = e.IsArchived,
                MeetupLink = e.MeetupLink,
                RejectionReason = e.RejectionReason,
                SubmittedAt = e.SubmittedAt,
                PublishedAt = e.PublishedAt,
                CreatedAt = e.CreatedAt,
                Medias = e.Medias.Select(m => new EventMediaResponse
                {
                    Id = m.Id,
                    Type = m.Type,
                    Url = m.Url,
                    Title = m.Title
                }).ToList(),
                Tags = e.EventTags.Select(et => new TagResponse
                {
                    Id = et.Tag.Id,
                    Name = et.Tag.Name,
                    Slug = et.Tag.Slug,
                    UsageCount = et.Tag.UsageCount
                }).ToList()
            })
            .ToListAsync();

        return new PaginatedResponse<EventResponse> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <summary>Approuve et publie un événement en attente (délègue à PublishAsync).</summary>
    public async Task<EventResponse?> ApproveAsync(Guid id)
    {
        return await PublishAsync(id);
    }

    /// <summary>Rejette un événement avec un motif (reste non publié).</summary>
    public async Task<EventResponse?> RejectAsync(Guid id, string reason)
    {
        var ev = await db.Events
            .Include(e => e.Medias)
            .Include(e => e.EventTags).ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;
        ev.IsPublished = false;
        ev.RejectionReason = reason;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapEvent(ev);
    }

    /// <summary>Inscrit un utilisateur à un événement si la capacité le permet (transaction BD).</summary>
    public async Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string avatarUrl = "")
    {
        var existing = await db.EventRegistrations.AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        if (existing) return null;

        using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var rows = await db.Events
                .Where(e => e.Id == eventId && e.RegisteredCount < e.Capacity)
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
            return MapRegistration(registration, ev.Title);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return null;
        }
    }

    /// <summary>Annule l'inscription et décrémente le compteur de participants.</summary>
    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        var reg = await db.EventRegistrations.FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
        if (reg is null) return false;

        using var tx = await db.Database.BeginTransactionAsync();
        await db.Events
            .Where(e => e.Id == eventId && e.RegisteredCount > 0)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.RegisteredCount, e => e.RegisteredCount - 1));

        db.EventRegistrations.Remove(reg);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }

    /// <summary>Liste des inscriptions à un événement.</summary>
    public async Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId)
    {
        var eventTitle = await db.Events.AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => e.Title)
            .FirstOrDefaultAsync() ?? "";

        return await db.EventRegistrations.AsNoTracking()
            .Where(r => r.EventId == eventId)
            .Select(r => MapRegistration(r, eventTitle))
            .ToListAsync();
    }

    private async Task AssignTags(Event ev, List<string> tagNames)
    {
        var names = tagNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count == 0) return;

        var slugs = names.Select(GenerateSlug).ToHashSet();
        var existingTags = await db.Tags.Where(t => slugs.Contains(t.Slug)).ToListAsync();
        var existingBySlug = existingTags.ToDictionary(t => t.Slug);

        foreach (var name in names)
        {
            var slug = GenerateSlug(name);
            if (!existingBySlug.TryGetValue(slug, out var tag))
            {
                tag = new Tag { Id = Guid.NewGuid(), Name = name, Slug = slug };
                db.Tags.Add(tag);
                existingBySlug[slug] = tag;
            }
            ev.EventTags.Add(new EventTag { EventId = ev.Id, TagId = tag.Id });
        }
    }

    private static EventResponse MapEvent(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Slug = e.Slug,
        Description = e.Description,
        Location = e.Location,
        EventType = e.EventType,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        CoverImageUrl = e.CoverImageUrl,
        CreatedBy = e.CreatedBy,
        SubmittedBy = e.CreatedBy,
        OrganizerName = e.OrganizerName,
        Capacity = e.Capacity,
        RegisteredCount = e.RegisteredCount,
        IsPublished = e.IsPublished,
        IsArchived = e.IsArchived,
        MeetupLink = e.MeetupLink,
        RejectionReason = e.RejectionReason,
        SubmittedAt = e.SubmittedAt,
        PublishedAt = e.PublishedAt,
        CreatedAt = e.CreatedAt,
        Medias = e.Medias.Select(m => new EventMediaResponse
        {
            Id = m.Id,
            Type = m.Type,
            Url = m.Url,
            Title = m.Title
        }).ToList(),
        GalleryImageUrls = e.Medias.Where(m => m.Type == "Image").Select(m => m.Url).ToList(),
        Speakers = e.Speakers.Select(s => new SpeakerResponse
        {
            Id = s.Id,
            EventId = s.EventId,
            UserId = s.UserId,
            Name = s.Name,
            Role = s.Role,
            AvatarUrl = s.AvatarUrl
        }).ToList(),
        Tags = e.EventTags.Select(et => new TagResponse
        {
            Id = et.Tag.Id,
            Name = et.Tag.Name,
            Slug = et.Tag.Slug,
            UsageCount = et.Tag.UsageCount
        }).ToList()
    };

    private static EventRegistrationResponse MapRegistration(EventRegistration r, string eventTitle) => new()
    {
        Id = r.Id,
        EventId = r.EventId,
        EventTitle = eventTitle,
        UserId = r.UserId,
        UserName = r.UserName,
        AvatarUrl = r.AvatarUrl,
        RegisteredAt = r.RegisteredAt,
        IsAttended = r.IsAttended,
        RegistrationStatus = r.RegistrationStatus
    };

    private static string GenerateSlug(string text) => SlugGenerator.Generate(text);
}
