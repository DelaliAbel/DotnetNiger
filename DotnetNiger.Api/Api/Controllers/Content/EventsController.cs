using DotnetNiger.Api.Application.Interfaces;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Content;

/// <summary>Contrôleur de gestion des événements.</summary>
[ApiController]
[Route("api/events")]
public class EventsController(
    IEventQueryService eventQuery,
    IEventCommandService eventCommand,
    IEventRegistrationService eventRegistration,
    IEventModerationService eventModeration) : BaseController
{
    /// <summary>Récupère la liste paginée des événements avec filtres optionnels.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await eventQuery.GetAllAsync(status, query, null, null, null, null, null, null, page, pageSize);
        return Success(result);
    }

    /// <summary>Récupère tous les événements (admin - tous statuts).</summary>
    [HttpGet("admin")]
    [Authorize(Policy = "admin.dashboard.view")]
    public async Task<IActionResult> GetAdminAll([FromQuery] string? status, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await eventQuery.GetAllAsync(status, query, null, null, null, null, null, null, page, pageSize);
        return Success(result);
    }

    /// <summary>Récupère les événements de l'utilisateur connecté.</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var userId = GetUserId();
        var result = await eventQuery.GetAllAsync(null, null, null, null, null, null, null, null, page, pageSize, userId);
        return Success(result);
    }

    /// <summary>Récupère un événement par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await eventQuery.GetByIdAsync(id);
        if (ev is null) return NotFound(Messages.Event.NotFound);
        if (ev.Status != nameof(EventStatus.Published) && !CanViewUnpublished(ev))
            return NotFound(Messages.Event.NotFound);
        return Success(ev);
    }

    /// <summary>Récupère un événement par son slug.</summary>
    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var ev = await eventQuery.GetBySlugAsync(slug);
        if (ev is null) return NotFound(Messages.Event.NotFound);
        if (ev.Status != nameof(EventStatus.Published) && !CanViewUnpublished(ev))
            return NotFound(Messages.Event.NotFound);
        return Success(ev);
    }

    private bool CanViewUnpublished(EventResponse ev)
    {
        if (!User.Identity?.IsAuthenticated == true) return false;
        var userId = GetUserId();
        return IsAdmin() || IsCollaborator() || ev.CreatedBy == userId;
    }

    /// <summary>Crée un nouvel événement.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var userId = GetUserId();
        try
        {
            var ev = await eventCommand.CreateAsync(request, userId, IsAdmin(), IsCollaborator());
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, new { success = true, data = ev, message = (string?)null });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Met à jour un événement existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        try
        {
            var ev = await eventCommand.UpdateAsync(id, request, GetUserId(), IsAdmin());
            if (ev is null) return NotFound(Messages.Event.NotFound);
            return Success(ev);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Supprime un événement.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await eventCommand.DeleteAsync(id, GetUserId(), IsAdmin());
            if (!deleted) return NotFound(Messages.Event.NotFound);
            return Success<object?>(null, Messages.Event.Deleted);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
    }

    /// <summary>Inscrit un utilisateur à un événement.</summary>
    [HttpPost("registrations")]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterEventRequest request)
    {
        var userId = GetUserId();
        var result = await eventRegistration.RegisterAsync(request.EventId, userId, GetUserName(), GetUserAvatar() ?? request.AvatarUrl);
        if (result is null) return BadRequest(Messages.Event.FullOrRegistered);
        return Success(result);
    }

    /// <summary>Annule l'inscription d'un utilisateur à un événement.</summary>
    [HttpDelete("{eventId:guid}/registrations")]
    [Authorize]
    public async Task<IActionResult> CancelRegistration(Guid eventId)
    {
        var cancelled = await eventRegistration.CancelRegistrationAsync(eventId, GetUserId());
        if (!cancelled) return NotFound(Messages.Event.RegistrationNotFound);
        return Success<object?>(null, Messages.Event.RegistrationCancelled);
    }

    /// <summary>Récupère les inscriptions d'un événement (organisateur ou modérateur/admin uniquement).</summary>
    [HttpGet("{eventId:guid}/registrations")]
    [Authorize]
    public async Task<IActionResult> GetRegistrations(Guid eventId)
    {
        var ev = await eventQuery.GetByIdAsync(eventId);
        if (ev is null) return NotFound(Messages.Event.NotFound);

        var userId = GetUserId();
        if (ev.CreatedBy != userId && !IsAdmin() && !IsCollaborator())
            return Failure(Messages.Event.RegistrationsForbidden, 403);

        var registrations = await eventQuery.GetRegistrationsAsync(eventId);
        return Success(registrations);
    }

    /// <summary>Récupère les événements en attente de modération.</summary>
    [HttpGet("pending")]
    [Authorize(Policy = "content.events.moderate")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        return Success(await eventQuery.GetPendingEventsAsync(page, pageSize));
    }

    /// <summary>Publie un événement (le rend visible).</summary>
    [HttpPatch("{id:guid}/publish")]
    [Authorize(Policy = "content.events.moderate")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var ev = await eventModeration.PublishAsync(id);
        if (ev is null) return NotFound(Messages.Event.NotFound);
        return Success(ev);
    }

    /// <summary>Dépublie un événement (le rend invisible).</summary>
    [HttpPatch("{id:guid}/unpublish")]
    [Authorize(Policy = "content.events.moderate")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var ev = await eventModeration.UnpublishAsync(id);
        if (ev is null) return NotFound(Messages.Event.NotFound);
        return Success(ev);
    }

    /// <summary>Approuve un événement soumis.</summary>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Policy = "content.events.approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ev = await eventModeration.ApproveAsync(id);
        if (ev is null) return NotFound(Messages.Event.NotFound);
        return Success(ev);
    }

    /// <summary>Rejette un événement soumis avec une raison.</summary>
    [HttpPatch("{id:guid}/reject")]
    [Authorize(Policy = "content.events.approve")]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(Messages.Certificate.RejectReasonRequired);
        var ev = await eventModeration.RejectAsync(id, reason);
        if (ev is null) return NotFound(Messages.Event.NotFound);
        return Success(ev);
    }
}
