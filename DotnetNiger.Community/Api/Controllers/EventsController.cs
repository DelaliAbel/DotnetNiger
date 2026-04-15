using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Api.Services;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les événements
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
public class EventsController : ApiControllerBase
{
    private readonly IEventService _eventService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunityRequestMapper _requestMapper;

    public EventsController(
        IEventService eventService,
        ICurrentUserService currentUserService,
        ICommunityRequestMapper requestMapper)
    {
        _eventService = eventService;
        _currentUserService = currentUserService;
        _requestMapper = requestMapper;
    }

    /// <summary>
    /// Récupérer tous les événements
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre d'événements par page (par défaut 10)</param>
    /// <returns>Liste paginée des événements</returns>
    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Parametres de pagination invalides");

        var events = await _eventService.GetAllEventsAsync(page, pageSize);
        return Success(events, meta: new { page, pageSize, total = events.Count() });
    }

    /// <summary>
    /// Récupérer un événement par ID
    /// </summary>
    /// <param name="id">ID de l'événement</param>
    /// <returns>Détails de l'événement</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(string id)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var @event = await _eventService.GetEventByIdAsync(eventId);
        if (@event == null)
            return NotFoundProblem("Evenement non trouve");

        return Success(@event);
    }

    /// <summary>
    /// Récupérer les événements à venir
    /// </summary>
    /// <param name="limit">Nombre d'événements (par défaut 10)</param>
    /// <returns>Événements à venir</returns>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Invalid pagination parameters");

        var events = await _eventService.GetUpcomingEventsAsync(pageSize);
        var paginatedEvents = events
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Success(paginatedEvents, meta: new { page, pageSize, total = events.Count() });
    }

    /// <summary>
    /// Créer un nouvel événement
    /// </summary>
    /// <param name="request">Données de l'événement</param>
    /// <returns>Événement créé</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Title))
            return BadRequestProblem("Titre requis");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var @event = _requestMapper.MapToEvent(request, currentUserId);

        var createdEvent = await _eventService.CreateEventAsync(@event);
        return CreatedSuccess(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent, "Evenement cree avec succes");
    }

    /// <summary>
    /// Mettre à jour un événement
    /// </summary>
    /// <param name="id">ID de l'événement</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Événement mis à jour</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] UpdateEventRequest request)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var @event = await _eventService.GetEventByIdAsync(eventId);
        if (@event == null)
            return NotFoundProblem("Evenement non trouve");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var canModerate = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (@event.CreatedBy != currentUserId && !canModerate)
            return Forbid();

        _requestMapper.ApplyEventUpdates(@event, request);
        var updatedEvent = await _eventService.UpdateEventAsync(@event);
        return Success(updatedEvent, "Evenement mis a jour avec succes");
    }

    /// <summary>
    /// Supprimer un événement
    /// </summary>
    /// <param name="id">ID de l'événement</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var @event = await _eventService.GetEventByIdAsync(eventId);
        if (@event == null)
            return NotFoundProblem("Evenement non trouve");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var canModerate = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (@event.CreatedBy != currentUserId && !canModerate)
            return Forbid();

        var deleted = await _eventService.DeleteEventAsync(eventId);
        if (!deleted)
            return NotFoundProblem("Evenement non trouve");

        return SuccessMessage("Evenement supprime avec succes");
    }

    // ====== Event Registration Endpoints - SECURE ======

    /// <summary>
    /// Enregistrer l'utilisateur actuel à un événement
    /// SÉCURISÉ: UserId vient du JWT token, pas du client
    /// </summary>
    /// <param name="request">Données d'enregistrement (juste EventId)</param>
    /// <returns>Détails de l'enregistrement</returns>
    [HttpPost("registrations")]
    [Authorize]
    public async Task<IActionResult> RegisterToEvent([FromBody] RegisterEventRequest request)
    {
        if (request == null || request.EventId == Guid.Empty)
            return BadRequestProblem("EventId requis et doit être valide");

        try
        {
            // SÉCURISÉ: Extraire UserId du JWT token (pas du client)
            var userId = RequireAuthenticatedUserId();

            var registration = await _eventService.RegisterToEventAsync(request.EventId, userId);

            return CreatedSuccess(
                nameof(GetEventById),
                new { id = request.EventId },
                registration,
                "Enregistrement à l'événement réussi");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Non autorisé",
                Detail = "Vous devez être connecté pour vous enregistrer à un événement",
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    /// <summary>
    /// Annuler l'enregistrement de l'utilisateur actuel à un événement
    /// SÉCURISÉ: Vérifie que l'utilisateur annule son propre enregistrement
    /// </summary>
    /// <param name="eventId">ID de l'événement</param>
    /// <returns>Confirmation d'annulation</returns>
    [HttpDelete("{eventId}/registrations")]
    [Authorize]
    public async Task<IActionResult> CancelRegistration(string eventId)
    {
        var eventIdParsed = ParseGuidOrThrow(eventId, nameof(eventId), "ID de l'événement invalide");

        try
        {
            // SÉCURISÉ: Extraire UserId du JWT token
            var userId = RequireAuthenticatedUserId();

            var result = await _eventService.CancelRegistrationAsync(eventIdParsed, userId);

            if (result)
                return SuccessMessage("Enregistrement annulé avec succès");

            return NotFoundProblem("Enregistrement non trouvé");
        }
        catch (InvalidOperationException ex)
        {
            return NotFoundProblem(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Non autorisé",
                Detail = "Vous devez être connecté pour annuler votre enregistrement",
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    /// <summary>
    /// Récupérer les enregistrements d'un événement (admin)
    /// </summary>
    /// <param name="eventId">ID de l'événement</param>
    /// <returns>Liste des enregistrements</returns>
    [HttpGet("{eventId}/registrations")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetEventRegistrations(string eventId)
    {
        var eventIdParsed = ParseGuidOrThrow(eventId, nameof(eventId), "ID de l'événement invalide");

        try
        {
            var registrations = await _eventService.GetEventRegistrationsAsync(eventIdParsed);
            return Success(registrations, meta: new { count = registrations.Count() });
        }
        catch (InvalidOperationException ex)
        {
            return NotFoundProblem(ex.Message);
        }
    }
}
