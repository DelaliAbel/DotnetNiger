using Asp.Versioning;
using DotnetNiger.Community.Application;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des événements de la communauté (meetups, conférences, ateliers).</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController(IEventService eventService, IProfileService profileService) : BaseController
{
    /// <summary>Recherche et filtre les événements avec pagination.</summary>
    /// <param name="published">Filtre publication ("true"/"false").</param>
    /// <param name="past">Filtre passé ("true"/"false").</param>
    /// <param name="eventType">Type d'événement (meetup, conference, workshop...).</param>
    /// <param name="query">Recherche textuelle.</param>
    /// <param name="tag">Filtre par tag.</param>
    /// <param name="startDateFrom">Date de début minimum.</param>
    /// <param name="startDateTo">Date de début maximum.</param>
    /// <param name="submitterId">Filtre par créateur.</param>
    /// <param name="page">Page demandée.</param>
    /// <param name="pageSize">Taille de la page.</param>
    /// <param name="after">Curseur pour la pagination.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? published, [FromQuery] string? past, [FromQuery] string? eventType,
        [FromQuery] string? query, [FromQuery] string? tag,
        [FromQuery] DateTime? startDateFrom, [FromQuery] DateTime? startDateTo,
        [FromQuery] Guid? submitterId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] Guid? after = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await eventService.GetAllAsync(published, past, eventType, query, tag, startDateFrom, startDateTo, submitterId, page, pageSize, after);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Récupère les événements à venir.</summary>
    /// <param name="page">Page demandée.</param>
    /// <param name="pageSize">Taille de la page.</param>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var events = await eventService.GetUpcomingAsync(page, pageSize);
        return Ok(new { Success = true, Data = events });
    }

    /// <summary>Recherche un événement par son identifiant.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await eventService.GetByIdAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Recherche un événement par son slug.</summary>
    /// <param name="slug">Slug de l'événement.</param>
    [HttpGet("{slug:regex(^[[a-z0-9]]+(?:-[[a-z0-9]]+)*$)}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var ev = await eventService.GetBySlugAsync(slug);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Récupère les métadonnées Open Graph d'un événement pour le partage social.</summary>
    /// <param name="slug">Slug de l'événement.</param>
    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<OGMetadata>> GetOGBySlug(string slug)
    {
        var ev = await eventService.GetBySlugAsync(slug);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });

        return Ok(new ApiSuccessResponse<OGMetadata>
        {
            Data = new OGMetadata
            {
                Title = ev.Title,
                Description = ev.Description,
                ImageUrl = ev.CoverImageUrl,
                UpdatedAt = ev.UpdatedAt
            }
        });
    }

    /// <summary>Crée un nouvel événement. Les collaborateurs doivent avoir un certificat validé.</summary>
    /// <param name="request">Informations de l'événement.</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var userId = GetUserId();

        if (!IsAdmin())
        {
            if (!IsCollaborator())
                return Forbid();

            var hasCert = await profileService.HasApprovedCertificateAsync(userId);
            if (!hasCert)
                return BadRequest(new { Success = false, Message = Messages.Certificate.NeedValidCertificate });

            request.IsPublished = false;
        }

        var ev = await eventService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, new { Success = true, Data = ev });
    }

    /// <summary>Modifie un événement existant (auteur ou admin).</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="request">Nouvelles informations.</param>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateEventRequest request)
    {
        var userId = GetUserId();
        var ev = await eventService.UpdateAsync(id, request, userId, IsAdmin());
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Supprime un événement (auteur ou admin).</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await eventService.DeleteAsync(id, userId, IsAdmin());
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Message = Messages.Event.Deleted });
    }

    /// <summary>Inscrit l'utilisateur connecté à un événement.</summary>
    /// <param name="request">Identifiant de l'événement et avatar optionnel.</param>
    [HttpPost("registrations")]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterEventRequest request)
    {
        var userId = GetUserId();
        var userName = GetUserName();
        var avatarUrl = GetUserAvatar() ?? request.AvatarUrl;
        var result = await eventService.RegisterAsync(request.EventId, userId, userName, avatarUrl);
        if (result is null)
            return BadRequest(new { Success = false, Message = Messages.Event.FullOrRegistered });
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Annule l'inscription de l'utilisateur connecté à un événement.</summary>
    /// <param name="eventId">Identifiant de l'événement.</param>
    [HttpDelete("{eventId:guid}/registrations")]
    [Authorize]
    public async Task<IActionResult> CancelRegistration(Guid eventId)
    {
        var userId = GetUserId();
        var cancelled = await eventService.CancelRegistrationAsync(eventId, userId);
        if (!cancelled) return NotFound(new { Success = false, Message = Messages.Event.RegistrationNotFound });
        return Ok(new { Success = true, Message = Messages.Event.RegistrationCancelled });
    }

    /// <summary>Récupère la liste des inscriptions à un événement.</summary>
    /// <param name="eventId">Identifiant de l'événement.</param>
    [HttpGet("{eventId:guid}/registrations")]
    [Authorize]
    public async Task<IActionResult> GetRegistrations(Guid eventId)
    {
        var userId = GetUserId();
        var registrations = await eventService.GetRegistrationsAsync(eventId);
        return Ok(new { Success = true, Data = registrations });
    }

    /// <summary>Récupère les événements en attente de validation (réservé aux admins).</summary>
    /// <param name="page">Page demandée.</param>
    /// <param name="pageSize">Taille de la page.</param>
    [HttpGet("pending")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var events = await eventService.GetPendingEventsAsync(page, pageSize);
        return Ok(new { Success = true, Data = events });
    }

    /// <summary>Approuve un événement en attente (réservé aux admins).</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="comment">Commentaire optionnel d'approbation.</param>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Approve(Guid id, [FromQuery] string? comment = null)
    {
        var ev = await eventService.ApproveAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Rejette un événement avec une raison (réservé aux admins).</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="reason">Motif du rejet.</param>
    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { Success = false, Message = Messages.Certificate.RejectReasonRequired });

        var ev = await eventService.RejectAsync(id, reason);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }
}
