using Asp.Versioning;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des événements de la communauté.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController(
    IEventQueryService eventQuery,
    IEventCommandService eventCommand,
    IEventModerationService eventModeration,
    IEventRegistrationService eventRegistration) : BaseController
{
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
        var result = await eventQuery.GetAllAsync(published, past, eventType, query, tag, startDateFrom, startDateTo, submitterId, page, pageSize, after);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var events = await eventQuery.GetUpcomingAsync(page, pageSize);
        return Ok(new { Success = true, Data = events });
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var userId = GetUserId();
        var result = await eventQuery.GetAllAsync(null, null, null, null, null, null, null, userId, page, pageSize, null);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await eventQuery.GetByIdAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpGet("{slug:regex(^[[a-z0-9]]+(?:-[[a-z0-9]]+)*$)}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var ev = await eventQuery.GetBySlugAsync(slug);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<OGMetadata>> GetOGBySlug(string slug)
    {
        var ev = await eventQuery.GetBySlugAsync(slug);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new ApiSuccessResponse<OGMetadata>
        {
            Data = new OGMetadata { Title = ev.Title, Description = ev.Description, ImageUrl = ev.CoverImageUrl, UpdatedAt = ev.UpdatedAt }
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var userId = GetUserId();
        try
        {
            var ev = await eventCommand.CreateAsync(request, userId, IsAdmin(), IsCollaborator());
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, new { Success = true, Data = ev });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateEventRequest request)
    {
        var ev = await eventCommand.UpdateAsync(id, request, GetUserId(), IsAdmin());
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await eventCommand.DeleteAsync(id, GetUserId(), IsAdmin());
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Message = Messages.Event.Deleted });
    }

    [HttpPost("registrations")]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterEventRequest request)
    {
        var userId = GetUserId();
        var result = await eventRegistration.RegisterAsync(request.EventId, userId, GetUserName(), GetUserAvatar() ?? request.AvatarUrl);
        if (result is null) return BadRequest(new { Success = false, Message = Messages.Event.FullOrRegistered });
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("{eventId:guid}/registrations")]
    [Authorize]
    public async Task<IActionResult> CancelRegistration(Guid eventId)
    {
        var cancelled = await eventRegistration.CancelRegistrationAsync(eventId, GetUserId());
        if (!cancelled) return NotFound(new { Success = false, Message = Messages.Event.RegistrationNotFound });
        return Ok(new { Success = true, Message = Messages.Event.RegistrationCancelled });
    }

    [HttpGet("{eventId:guid}/registrations")]
    [Authorize]
    public async Task<IActionResult> GetRegistrations(Guid eventId)
    {
        var registrations = await eventQuery.GetRegistrationsAsync(eventId);
        return Ok(new { Success = true, Data = registrations });
    }

    [HttpGet("pending")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        return Ok(new { Success = true, Data = await eventQuery.GetPendingEventsAsync(page, pageSize) });
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ev = await eventModeration.ApproveAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { Success = false, Message = Messages.Certificate.RejectReasonRequired });
        var ev = await eventModeration.RejectAsync(id, reason);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }
}
