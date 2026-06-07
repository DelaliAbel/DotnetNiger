using System.Security.Claims;
using Asp.Versioning;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificationsController(IUserNotificationService notificationService) : ControllerBase
{
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetNotifications(Guid userId)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userId != currentUserId) return Forbid();
        var notifications = await notificationService.GetNotificationsAsync(userId);
        return Ok(new { Success = true, Data = notifications });
    }

    [HttpGet("{userId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid userId)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userId != currentUserId) return Forbid();
        var count = await notificationService.GetUnreadCountAsync(userId);
        return Ok(new { Success = true, Data = new { Count = count } });
    }

    [HttpPost("{userId:guid}")]
    public async Task<IActionResult> SendNotification(Guid userId, [FromBody] SendNotificationRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userId != currentUserId) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { Success = false, Message = "Message is required" });

        await notificationService.SendNotificationAsync(userId, request.Message);
        return Ok(new { Success = true, Message = "Notification sent" });
    }

    [HttpPatch("{userId:guid}/{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid userId, Guid notificationId)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userId != currentUserId) return Forbid();
        var marked = await notificationService.MarkAsReadAsync(userId, notificationId);
        if (!marked) return NotFound(new { Success = false, Message = "Notification not found" });
        return Ok(new { Success = true, Message = "Notification marked as read" });
    }

    [HttpPatch("{userId:guid}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userId != currentUserId) return Forbid();
        await notificationService.MarkAllAsReadAsync(userId);
        return Ok(new { Success = true, Message = "All notifications marked as read" });
    }

    public class SendNotificationRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
