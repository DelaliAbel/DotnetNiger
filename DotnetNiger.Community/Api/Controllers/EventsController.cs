using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Api.Services;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les événements
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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
    [HttpGet("upcoming/{limit:int?}")]
    public async Task<IActionResult> GetUpcomingEvents(int? limit)
    {
        var events = await _eventService.GetUpcomingEventsAsync(limit ?? 10);
        return Success(events);
    }

    /// <summary>
    /// Créer un nouvel événement
    /// </summary>
    /// <param name="request">Données de l'événement</param>
    /// <returns>Événement créé</returns>
    [HttpPost]
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
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] UpdateEventRequest request)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var @event = await _eventService.GetEventByIdAsync(eventId);
        if (@event == null)
            return NotFoundProblem("Evenement non trouve");

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
    public async Task<IActionResult> DeleteEvent(string id)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var deleted = await _eventService.DeleteEventAsync(eventId);
        if (!deleted)
            return NotFoundProblem("Evenement non trouve");

        return SuccessMessage("Evenement supprime avec succes");
    }
}



