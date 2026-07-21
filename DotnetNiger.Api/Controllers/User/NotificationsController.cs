using DotnetNiger.Api.Services;
using DotnetNiger.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.User;

[ApiController]
[Route("api/notification")]
[Authorize]
public class NotificationsController(IUserNotificationService notificationService) : BaseController
{
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetNotifications(Guid userId)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var notifications = await notificationService.GetNotificationsAsync(userId);
        return Ok(new { Success = true, Data = notifications });
    }

    [HttpGet("{userId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid userId)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var count = await notificationService.GetUnreadCountAsync(userId);
        return Ok(new { Success = true, Data = new { Count = count } });
    }

    [HttpPost("{userId:guid}")]
    public async Task<IActionResult> SendNotification(Guid userId, [FromBody] SendNotificationRequest request)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { Success = false, Message = Messages.Notification.MessageRequired });

        await notificationService.SendNotificationAsync(userId, request.Message);
        return Ok(new { Success = true, Message = Messages.Notification.Sent });
    }

    [HttpPatch("{userId:guid}/{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid userId, Guid notificationId)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var marked = await notificationService.MarkAsReadAsync(userId, notificationId);
        if (!marked) return NotFound(new { Success = false, Message = Messages.Notification.NotFound });
        return Ok(new { Success = true, Message = Messages.Notification.MarkedAsRead });
    }

    [HttpPatch("{userId:guid}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        await notificationService.MarkAllAsReadAsync(userId);
        return Ok(new { Success = true, Message = Messages.Notification.AllMarkedAsRead });
    }

    public class SendNotificationRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
