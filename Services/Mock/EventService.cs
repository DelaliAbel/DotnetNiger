using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Mock;

public class EventService : IEventService
{
    private List<EventDto> _events;
    private List<EventRegistrationDto> _registrations;

    public EventService()
    {
        _events = new List<EventDto>
        {
            new EventDto
            {
                Id = Guid.NewGuid(),
                Title = ".NET Niger Meetup #1",
                Slug = "dotnet-niger-meetup-1",
                Description = "Premier meetup de la communauté .NET Niger à Niamey. Venez découvrir les nouveautés de .NET 9 et échanger avec les développeurs locaux.",
                Location = "Niamey, Niger",
                EventType = "Physical",
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(10).AddHours(3),
                CoverImageUrl = "/Images/evenement.jpg",
                OrganizerName = "Équipe .NET Niger",
                Capacity = 50,
                RegisteredCount = 18,
                IsPublished = true,
                MeetupLink = "",
                Medias = new List<EventMediaDto>()
            },
            new EventDto
            {
                Id = Guid.NewGuid(),
                Title = "Workshop Blazor WebAssembly",
                Slug = "workshop-blazor-webassembly",
                Description = "Atelier pratique sur Blazor WebAssembly : créez votre première application SPA avec .NET.",
                Location = "Online",
                EventType = "Online",
                StartDate = DateTime.Now.AddDays(25),
                EndDate = DateTime.Now.AddDays(25).AddHours(4),
                CoverImageUrl = "/Images/evenement.jpg",
                OrganizerName = "Équipe .NET Niger",
                Capacity = 100,
                RegisteredCount = 42,
                IsPublished = true,
                MeetupLink = "https://meet.example.com/blazor-workshop",
                Medias = new List<EventMediaDto>()
            },
            new EventDto
            {
                Id = Guid.NewGuid(),
                Title = "Conférence ASP.NET Core & API REST",
                Slug = "conference-aspnet-core-api-rest",
                Description = "Conception d'API REST robustes avec ASP.NET Core, bonnes pratiques et retours d'expérience.",
                Location = "Niamey — Maison de la Jeunesse",
                EventType = "Hybrid",
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(-5).AddHours(5),
                CoverImageUrl = "/Images/evenement.jpg",
                OrganizerName = "Équipe .NET Niger",
                Capacity = 80,
                RegisteredCount = 80,
                IsPublished = true,
                MeetupLink = "https://meet.example.com/aspnet-conf",
                Medias = new List<EventMediaDto>
                {
                    new EventMediaDto { Id = Guid.NewGuid(), Type = "Image", Url = "/Images/evenement.jpg", Title = "Photo de l'événement" }
                }
            }
        };

        _registrations = new List<EventRegistrationDto>();
    }

    // ── Lecture ────────────────────────────────────────────────

    public async Task<List<EventDto>> GetAllEventsAsync()
    {
        return await Task.FromResult(
            _events.OrderByDescending(e => e.StartDate).ToList());
    }

    public async Task<List<EventDto>> GetPublishedEventsAsync()
    {
        return await Task.FromResult(
            _events.Where(e => e.IsPublished)
                   .OrderBy(e => e.StartDate)
                   .ToList());
    }

    public async Task<List<EventDto>> GetUpcomingEventsAsync()
    {
        return await Task.FromResult(
            _events.Where(e => e.IsPublished && e.StartDate >= DateTime.Now)
                   .OrderBy(e => e.StartDate)
                   .ToList());
    }

    public async Task<List<EventDto>> GetPastEventsAsync()
    {
        return await Task.FromResult(
            _events.Where(e => e.IsPublished && e.EndDate < DateTime.Now)
                   .OrderByDescending(e => e.StartDate)
                   .ToList());
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id)
    {
        var ev = _events.FirstOrDefault(e => e.Id == id);
        return await Task.FromResult(ev);
    }

    public async Task<EventDto?> GetEventBySlugAsync(string slug)
    {
        var ev = _events.FirstOrDefault(e => e.Slug == slug);
        return await Task.FromResult(ev);
    }

    public async Task<List<EventDto>> SearchEventsAsync(string query)
    {
        return await Task.FromResult(
            _events.Where(e =>
                    e.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.OrganizerName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.StartDate)
                .ToList());
    }

    public async Task<List<EventDto>> GetEventsByTypeAsync(string eventType)
    {
        return await Task.FromResult(
            _events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase) && e.IsPublished)
                   .OrderBy(e => e.StartDate)
                   .ToList());
    }

    // ── Création / Mise à jour / Suppression ───────────────────

    public async Task<EventDto> CreateEventAsync(CreateEventRequest request)
    {
        var newEvent = new EventDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            Location = request.Location,
            EventType = request.EventType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CoverImageUrl = string.IsNullOrEmpty(request.CoverImageUrl)
                ? "/Images/evenement.jpg"
                : request.CoverImageUrl,
            OrganizerName = "Admin",
            Capacity = request.Capacity,
            RegisteredCount = 0,
            IsPublished = request.IsPublished,
            IsArchived = request.IsArchived,
            MeetupLink = request.MeetupLink,
            Medias = new List<EventMediaDto>()
        };

        _events.Add(newEvent);
        return await Task.FromResult(newEvent);
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
        ev.StartDate = request.StartDate;
        ev.EndDate = request.EndDate;
        ev.CoverImageUrl = request.CoverImageUrl;
        ev.Capacity = request.Capacity;
        ev.IsPublished = request.IsPublished;
        ev.IsArchived = request.IsArchived;
        ev.MeetupLink = request.MeetupLink;

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

    // ── Inscriptions ───────────────────────────────────────────

    public async Task<EventRegistrationDto?> RegisterToEventAsync(RegisterEventRequest request, Guid userId, string userName)
    {
        var ev = _events.FirstOrDefault(e => e.Id == request.EventId);
        if (ev is null || ev.RegisteredCount >= ev.Capacity)
            return await Task.FromResult<EventRegistrationDto?>(null);

        var alreadyRegistered = _registrations.Any(r => r.EventId == request.EventId && r.UserId == userId);
        if (alreadyRegistered)
            return await Task.FromResult<EventRegistrationDto?>(null);

        var registration = new EventRegistrationDto
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            EventTitle = ev.Title,
            UserId = userId,
            UserName = userName,
            RegisteredAt = DateTime.Now,
            IsAttended = false,
            RegistrationStatus = "Confirmed"
        };

        _registrations.Add(registration);
        ev.RegisteredCount++;

        return await Task.FromResult<EventRegistrationDto?>(registration);
    }

    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        var reg = _registrations.FirstOrDefault(r => r.EventId == eventId && r.UserId == userId);
        if (reg is null) return await Task.FromResult(false);

        _registrations.Remove(reg);
        var ev = _events.FirstOrDefault(e => e.Id == eventId);
        if (ev is not null) ev.RegisteredCount--;

        return await Task.FromResult(true);
    }

    public async Task<List<EventRegistrationDto>> GetRegistrationsByEventAsync(Guid eventId)
    {
        return await Task.FromResult(
            _registrations.Where(r => r.EventId == eventId).ToList());
    }

    // ── Utilitaires ────────────────────────────────────────────

    private static string GenerateSlug(string title)
    {
        return title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("à", "a").Replace("â", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
            .Replace("î", "i").Replace("ï", "i")
            .Replace("ô", "o")
            .Replace("ù", "u").Replace("û", "u")
            .Replace("ç", "c")
            .Replace("'", "-").Replace("\"", "")
            .Replace(",", "").Replace(".", "")
            .Replace("?", "").Replace("!", "").Replace("#", "");
    }
}
