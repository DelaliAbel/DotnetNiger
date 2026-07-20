using DotnetNiger.Infrastructure.Services;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers.Content;

[ApiController]
[Route("api/[controller]")]
public class EventsController(
    IEventQueryService eventQuery,
    IEventCommandService eventCommand,
    IEventRegistrationService eventRegistration,
    IEventModerationService eventModeration) : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await eventQuery.GetAllAsync(status, query, null, null, null, null, null, null, page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await eventQuery.GetByIdAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        try
        {
            var ev = await eventCommand.UpdateAsync(id, request, GetUserId(), IsAdmin());
            if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
            return Ok(new { Success = true, Data = ev });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
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

    [HttpPatch("{id:guid}/publish")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Publish(Guid id)
    {
        var ev = await eventModeration.PublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpPatch("{id:guid}/unpublish")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var ev = await eventModeration.UnpublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
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
