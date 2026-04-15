using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

public class EventService : IEventService
{
    private readonly IEventPersistence _eventRepository;
    private readonly IEventRegistrationPersistence _registrationRepository;
    private readonly ISlugGenerator _slugGenerator;

    public EventService(
        IEventPersistence eventRepository,
        IEventRegistrationPersistence registrationRepository,
        ISlugGenerator slugGenerator)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
        _slugGenerator = slugGenerator;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(int page = ValidationConstants.DefaultPage, int pageSize = ValidationConstants.DefaultPageSize)
    {
        // Server-side pagination: Query executed in database, not on client
        // Proper database-side Skip/Take prevents loading entire table into memory
        page = Math.Max(1, page);
        pageSize = Math.Min(pageSize, ValidationConstants.MaxPageSize); // Cap at 100 for safety

        return await _eventRepository.GetPagedAsync(page, pageSize);
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        return await _eventRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10)
    {
        return await _eventRepository.GetUpcomingEventsAsync(limit);
    }

    public async Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10)
    {
        return await _eventRepository.GetPastEventsAsync(limit);
    }

    public async Task<Event> CreateEventAsync(Event @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event), "Event cannot be null");

        @event.Id = Guid.NewGuid();
        @event.CreatedAt = DateTime.UtcNow;
        @event.Slug = _slugGenerator.Generate(@event.Title);
        return await _eventRepository.AddAsync(@event);
    }

    public async Task<Event> UpdateEventAsync(Event @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event), "Event cannot be null");

        if (@event.Id == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty", nameof(@event));

        @event.UpdatedAt = DateTime.UtcNow;
        @event.Slug = _slugGenerator.Generate(@event.Title);
        return await _eventRepository.UpdateAsync(@event);
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        return await _eventRepository.DeleteAsync(id);
    }

    // ====== Event Registration Methods - SECURE ======

    /// <summary>
    /// Enregistrer l'utilisateur actuel (depuis JWT) à un événement
    /// SÉCURISÉ: UserId vient du JWT token, pas du client
    /// </summary>
    public async Task<EventRegistrationResponse> RegisterToEventAsync(Guid eventId, Guid userId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        // Vérifier que l'événement existe
        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null)
            throw new InvalidOperationException($"Event with ID {eventId} not found");

        // Vérifier que l'utilisateur n'est pas déjà enregistré
        if (await _registrationRepository.IsUserRegisteredAsync(eventId, userId))
            throw new InvalidOperationException("User is already registered to this event");

        // Créer l'enregistrement
        var registration = new EventRegistration
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            RegisteredAt = DateTime.UtcNow,
            IsAttended = false,
            RegistrationStatus = "Registered"
        };

        var createdRegistration = await _registrationRepository.AddAsync(registration);
        return MapToDto(createdRegistration, @event.Title);
    }

    /// <summary>
    /// Annuler l'enregistrement avec vérification de propriété
    /// SÉCURISÉ: Vérifie que l'utilisateur annule son propre enregistrement
    /// </summary>
    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        // Récupérer l'enregistrement existant
        var registration = await _registrationRepository.GetUserRegistrationAsync(eventId, userId);
        if (registration == null)
            throw new InvalidOperationException("Registration not found");

        // Marquer comme annulé au lieu de supprimer (soft delete)
        registration.RegistrationStatus = "Cancelled";
        await _registrationRepository.UpdateAsync(registration);
        return true;
    }

    /// <summary>
    /// Récupérer les enregistrements d'un événement
    /// </summary>
    public async Task<IEnumerable<EventRegistrationResponse>> GetEventRegistrationsAsync(Guid eventId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));

        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null)
            throw new InvalidOperationException($"Event with ID {eventId} not found");

        var registrations = await _registrationRepository.GetEventRegistrationsAsync(eventId);
        return registrations.Select(r => MapToDto(r, @event.Title)).ToList();
    }

    /// <summary>
    /// Récupérer les enregistrements d'un utilisateur
    /// </summary>
    public async Task<IEnumerable<EventRegistrationResponse>> GetUserRegistrationsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var registrations = await _registrationRepository.GetUserRegistrationsAsync(userId);

        // Enrichir avec les titres des événements
        var registrationDtos = new List<EventRegistrationResponse>();
        foreach (var reg in registrations)
        {
            var @event = await _eventRepository.GetByIdAsync(reg.EventId);
            registrationDtos.Add(MapToDto(reg, @event?.Title ?? "Unknown Event"));
        }

        return registrationDtos;
    }

    // ====== Helper Methods ======

    /// <summary>
    /// Mapper EventRegistration vers EventRegistrationDto
    /// </summary>
    private static EventRegistrationResponse MapToDto(EventRegistration registration, string eventTitle)
    {
        return new EventRegistrationResponse
        {
            Id = registration.Id,
            EventId = registration.EventId,
            EventTitle = eventTitle,
            UserId = registration.UserId,
            UserName = string.Empty, // Sera rempli par le controller si nécessaire
            RegisteredAt = registration.RegisteredAt,
            IsAttended = registration.IsAttended,
            RegistrationStatus = registration.RegistrationStatus
        };
    }
}
