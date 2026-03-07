using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les événements
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
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
            return BadRequest(new { message = "Paramètres de pagination invalides" });

        try
        {
            var events = await _eventService.GetAllEventsAsync(page, pageSize);
            return Ok(new
            {
                page = page,
                pageSize = pageSize,
                total = events.Count(),
                data = events
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des événements", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer un événement par ID
    /// </summary>
    /// <param name="id">ID de l'événement</param>
    /// <returns>Détails de l'événement</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest(new { message = "ID de l'événement invalide" });

        try
        {
            var @event = await _eventService.GetEventByIdAsync(eventId);
            if (@event == null)
                return NotFound(new { message = "Événement non trouvé" });

            return Ok(@event);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'événement", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer les événements à venir
    /// </summary>
    /// <param name="limit">Nombre d'événements (par défaut 10)</param>
    /// <returns>Événements à venir</returns>
    [HttpGet("upcoming/{limit:int?}")]
    public async Task<IActionResult> GetUpcomingEvents(int? limit)
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync(limit ?? 10);
            return Ok(new { data = events });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des événements à venir", error = ex.Message });
        }
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
            return BadRequest(new { message = "Titre requis" });

        if (!this.TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(new { message = "Utilisateur non authentifie", details = "Claim user ou header X-User-Id requis" });

        try
        {
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Slug = request.Title.ToLower().Replace(" ", "-"),
                Description = request.Description ?? string.Empty,
                Location = request.Location ?? string.Empty,
                EventType = "Physical",
                StartDate = request.StartDate ?? DateTime.UtcNow.AddDays(7),
                EndDate = request.EndDate ?? DateTime.UtcNow.AddDays(8),
                CreatedBy = currentUserId,
                Capacity = 500,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdEvent = await _eventService.CreateEventAsync(@event);
            return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création de l'événement", error = ex.Message });
        }
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
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest(new { message = "ID de l'événement invalide" });

        try
        {
            var @event = await _eventService.GetEventByIdAsync(eventId);
            if (@event == null)
                return NotFound(new { message = "Événement non trouvé" });

            if (!string.IsNullOrEmpty(request.Title))
                @event.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Description))
                @event.Description = request.Description;
            if (request.StartDate.HasValue)
                @event.StartDate = request.StartDate.Value;

            var updatedEvent = await _eventService.UpdateEventAsync(@event);
            return Ok(updatedEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de l'événement", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer un événement
    /// </summary>
    /// <param name="id">ID de l'événement</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest(new { message = "ID de l'événement invalide" });

        try
        {
            var deleted = await _eventService.DeleteEventAsync(eventId);
            if (!deleted)
                return NotFound(new { message = "Événement non trouvé" });

            return Ok(new { message = "Événement supprimé avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression de l'événement", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO pour créer un événement
/// </summary>
public class CreateEventRequest
{
    /// <summary>Titre de l'événement</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Description de l'événement</summary>
    public string? Description { get; set; }
    /// <summary>Date de début</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Date de fin</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>Localisation</summary>
    public string? Location { get; set; }
    /// <summary>Tags de l'événement</summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// DTO pour mettre à jour un événement
/// </summary>
public class UpdateEventRequest
{
    /// <summary>Titre de l'événement</summary>
    public string? Title { get; set; }
    /// <summary>Description de l'événement</summary>
    public string? Description { get; set; }
    /// <summary>Date de début</summary>
    public DateTime? StartDate { get; set; }
}
