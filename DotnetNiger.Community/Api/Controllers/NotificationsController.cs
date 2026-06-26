using Asp.Versioning;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des notifications utilisateur.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificationsController(IUserNotificationService notificationService) : BaseController
{
    /// <summary>Récupère les notifications d'un utilisateur.</summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetNotifications(Guid userId)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var notifications = await notificationService.GetNotificationsAsync(userId);
        return Ok(new { Success = true, Data = notifications });
    }

    /// <summary>Retourne le nombre de notifications non lues.</summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    [HttpGet("{userId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid userId)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var count = await notificationService.GetUnreadCountAsync(userId);
        return Ok(new { Success = true, Data = new { Count = count } });
    }

    /// <summary>Envoie une notification à un utilisateur.</summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="request">Message de la notification.</param>
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

    /// <summary>Marque une notification comme lue.</summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="notificationId">Identifiant de la notification.</param>
    [HttpPatch("{userId:guid}/{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid userId, Guid notificationId)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var marked = await notificationService.MarkAsReadAsync(userId, notificationId);
        if (!marked) return NotFound(new { Success = false, Message = Messages.Notification.NotFound });
        return Ok(new { Success = true, Message = Messages.Notification.MarkedAsRead });
    }

    /// <summary>Marque toutes les notifications de l'utilisateur comme lues.</summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
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
