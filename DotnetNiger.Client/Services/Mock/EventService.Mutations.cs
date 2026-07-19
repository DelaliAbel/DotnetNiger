using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;
using DotnetNiger.Client.Services.Helpers;

namespace DotnetNiger.Client.Services.Mock;

public partial class EventService
{
    // ---- Création / Mise à jour / Suppression -------------------------------------------------

    public async Task<EventDto?> CreateEventAsync(CreateEventRequest request, Guid currentUserId, bool isAdmin)
    {
        await Task.Delay(500); // simuler appel API

        var resolvedIsAdmin = isAdmin || await _authService.IsAdminAsync();

        var slug = request.Title.ToLower().Replace(" ", "-");
        var now = DateTime.Now;

        var newEvent = new EventDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = slug,
            Description = request.Description,
            Location = request.Location,
            EventType = request.EventType,
            Category = request.Category,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CoverImageUrl = request.CoverImageUrl ?? "/images/events/default.jpg",
            Capacity = request.Capacity,
            MeetupLink = request.MeetupLink ?? "",
            Medias = request.GalleryImageUrls.Select(url => new EventMediaDto
            {
                Id = Guid.NewGuid(),
                Type = "Image",
                Url = url,
                Title = "Galerie"
            }).ToList(),
            GalleryImageUrls = request.GalleryImageUrls,
            Speakers = request.Speakers?.Select(s => new SpeakerDto
            {
                UserId = s.UserId,
                Name = s.Name,
                Role = s.Role,
                AvatarUrl = s.AvatarUrl
            }).ToList() ?? new(),
            CreatedBy = currentUserId,
            OrganizerName = (await _authService.GetCurrentUserAsync())?.FullName ?? "Organisateur",
            RegisteredCount = 0,
            SubmittedBy = currentUserId,
            SubmittedAt = now
        };

        if (resolvedIsAdmin)
        {
            // Admin : publication immédiate
            newEvent.IsPublished = true;
            newEvent.PublishedAt = now;
        }
        else
        {
            // Membre : en attente de validation
            newEvent.IsPublished = false;
            newEvent.PublishedAt = null;
        }

        _events.Add(newEvent);
        return newEvent;
    }

    public async Task<List<EventDto>> GetPendingEventsAsync()
    {
        await Task.Delay(800);
        return _events.Where(e => !e.IsPublished).ToList();
    }

    public async Task<bool> ApproveEventAsync(Guid eventId, string? adminComment = null)
    {
        await Task.Delay(500);
        var evt = _events.FirstOrDefault(e => e.Id == eventId);
        if (evt == null || evt.IsPublished) return false;

        evt.IsPublished = true;
        evt.PublishedAt = DateTime.Now;
        // Optionnel : stocker le commentaire admin (à ajouter dans DTO si besoin)
        return true;
    }

    public async Task<bool> RejectEventAsync(Guid eventId, string reason)
    {
        await Task.Delay(500);
        var evt = _events.FirstOrDefault(e => e.Id == eventId);
        if (evt == null || evt.IsPublished) return false;

        // Stocker la raison puis supprimer l'événement rejeté des listes visibles
        evt.RejectionReason = reason;

        // notifier l'auteur de l'événement
        var submitterId = evt.SubmittedBy;
        var message = $"Votre événement '{evt.Title}' a été rejeté : {reason}";
        await _notificationService.SendNotificationAsync(submitterId, message);

        _events.Remove(evt);

        // Supprimer aussi les inscriptions associées
        _registrations.RemoveAll(r => r.EventId == eventId);

        return true;
    }

    public async Task<List<EventDto>> GetEventsBySubmitterAsync(Guid userId)
    {
        await Task.Delay(800);
        return _events.Where(e => e.SubmittedBy == userId).OrderByDescending(e => e.SubmittedAt).ToList();
    }

    public async Task<List<EventDto>> GetMyEventsAsync()
    {
        await Task.Delay(800);
        var user = await _authService.GetCurrentUserAsync();
        if (user is null) return new();
        return _events.Where(e => e.SubmittedBy == user.Id).OrderByDescending(e => e.SubmittedAt).ToList();
    }

    public async Task<EventDto?> UpdateEventAsync(Guid id, CreateEventRequest request)
    {
        var ev = _events.FirstOrDefault(e => e.Id == id);
        if (ev is null) return await Task.FromResult<EventDto?>(null);

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
        ev.GalleryImageUrls = request.GalleryImageUrls;
        ev.Medias = request.GalleryImageUrls.Select(url => new EventMediaDto
        {
            Id = Guid.NewGuid(),
            Type = "Image",
            Url = url,
            Title = "Galerie"
        }).ToList();

        ev.Speakers = request.Speakers?.Select(s => new SpeakerDto
        {
            UserId = s.UserId,
            Name = s.Name,
            Role = s.Role,
            AvatarUrl = s.AvatarUrl
        }).ToList() ?? new();

        return await Task.FromResult<EventDto?>(ev);
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        var ev = _events.FirstOrDefault(e => e.Id == id);
        if (ev is null) return await Task.FromResult(false);

        _events.Remove(ev);
        return await Task.FromResult(true);
    }

    public async Task<bool> TogglePublishAsync(Guid id)
    {
        var ev = _events.FirstOrDefault(e => e.Id == id);
        if (ev is null) return await Task.FromResult(false);

        ev.IsPublished = !ev.IsPublished;
        return await Task.FromResult(true);
    }

    private static string GenerateSlug(string title)
        => StringHelper.GenerateSlug(title);
}
