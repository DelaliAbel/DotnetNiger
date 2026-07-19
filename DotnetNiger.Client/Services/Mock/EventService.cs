using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Auth;
using DotnetNiger.Client.Services.Contracts;
using DotnetNiger.Client.Services.Helpers;

namespace DotnetNiger.Client.Services.Mock;

public partial class EventService : IEventService
{
    private readonly IAuthService _authService;
    private List<EventDto> _events;
    private List<EventRegistrationDto> _registrations;

    private readonly INotificationService _notificationService;

    public EventService(IAuthService authService, INotificationService notificationService)
    {
        _authService = authService;
        _notificationService = notificationService;

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
                IsPublished = false,
                MeetupLink = "",
                Medias = new List<EventMediaDto>(),

                SubmittedBy = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            SubmittedAt = DateTime.Now.AddDays(-10),
            PublishedAt = DateTime.Now.AddDays(-10)
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
                Medias = new List<EventMediaDto>(),
                SubmittedBy = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            SubmittedAt = DateTime.Now.AddDays(-10),
            PublishedAt = DateTime.Now.AddDays(-10),
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
                },
                GalleryImageUrls = new List<string> { "/Images/evenement.jpg" }
            }
        };

        _registrations = new List<EventRegistrationDto>();
    }

    // ---- Lecture --------------------------------------------------------------------------------

    public async Task<List<EventDto>> GetAllEventsAsync()
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _events.OrderByDescending(e => e.StartDate).ToList());
    }

    public async Task<List<EventDto>> GetPublishedEventsAsync()
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _events.Where(e => e.IsPublished)
                   .OrderBy(e => e.StartDate)
                   .ToList());
    }

    public async Task<List<EventDto>> GetUpcomingEventsAsync()
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _events.Where(e => e.IsPublished && e.StartDate >= DateTime.Now)
                   .OrderBy(e => e.StartDate)
                   .ToList());
    }

    public async Task<List<EventDto>> GetPastEventsAsync()
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _events.Where(e => e.IsPublished && e.EndDate < DateTime.Now)
                   .OrderByDescending(e => e.StartDate)
                   .ToList());
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id)
    {
        await Task.Delay(800);
        var ev = _events.FirstOrDefault(e => e.Id == id);
        return await Task.FromResult(ev);
    }

    public async Task<EventDto?> GetEventBySlugAsync(string slug)
    {
        await Task.Delay(800);
        var ev = _events.FirstOrDefault(e => e.Slug == slug);
        return await Task.FromResult(ev);
    }

    public async Task<List<EventDto>> SearchEventsAsync(string query)
    {
        await Task.Delay(800);
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
        await Task.Delay(800);
        return await Task.FromResult(
            _events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase) && e.IsPublished)
                   .OrderBy(e => e.StartDate)
                   .ToList());
    }
}
