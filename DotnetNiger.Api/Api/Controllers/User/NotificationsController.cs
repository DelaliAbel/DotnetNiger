using System.Threading;
using DotnetNiger.Api.Application.Interfaces;
using DotnetNiger.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.User;

/// <summary>Contrôleur de gestion des notifications utilisateur.</summary>
[ApiController]
[Route("api/notification")]
[Authorize]
public class NotificationsController(IUserNotificationService notificationService) : BaseController
{
    /// <summary>Récupère les notifications d'un utilisateur.</summary>
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetNotifications(Guid userId, CancellationToken ct = default)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var notifications = await notificationService.GetNotificationsAsync(userId, ct);
        return Success(notifications);
    }

    /// <summary>Récupère le nombre de notifications non lues.</summary>
    [HttpGet("{userId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid userId, CancellationToken ct = default)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var count = await notificationService.GetUnreadCountAsync(userId, ct);
        return Success(new { Count = count });
    }

    /// <summary>Envoie une notification à un utilisateur.</summary>
    [HttpPost("{userId:guid}")]
    public async Task<IActionResult> SendNotification(Guid userId, [FromBody] SendNotificationRequest request, CancellationToken ct = default)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(Messages.Notification.MessageRequired);

        await notificationService.SendNotificationAsync(userId, request.Message, ct);
        return Success<object?>(null, Messages.Notification.Sent);
    }

    /// <summary>Marque une notification comme lue.</summary>
    [HttpPatch("{userId:guid}/{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid userId, Guid notificationId, CancellationToken ct = default)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        var marked = await notificationService.MarkAsReadAsync(userId, notificationId, ct);
        if (!marked) return NotFound(Messages.Notification.NotFound);
        return Success<object?>(null, Messages.Notification.MarkedAsRead);
    }

    /// <summary>Marque toutes les notifications d'un utilisateur comme lues.</summary>
    [HttpPatch("{userId:guid}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId, CancellationToken ct = default)
    {
        var currentUserId = GetUserId();
        if (userId != currentUserId) return Forbid();
        await notificationService.MarkAllAsReadAsync(userId, ct);
        return Success<object?>(null, Messages.Notification.AllMarkedAsRead);
    }

    /// <summary>Requête pour l'envoi d'une notification.</summary>
    public class SendNotificationRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
