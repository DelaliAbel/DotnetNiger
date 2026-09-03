using System.Threading;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de gestion de la newsletter.</summary>
[ApiController]
[Route("api/newsletter")]
[EnableRateLimiting("default")]
public class NewsletterController(INewsletterService newsletterService) : BaseController
{
    /// <summary>Inscrit un abonné à la newsletter (double opt-in).</summary>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request, CancellationToken ct = default)
    {
        var result = await newsletterService.SubscribeAsync(request, ct);
        return Success(result);
    }

    /// <summary>Confirme l'inscription d'un abonné via son token (double opt-in).</summary>
    [HttpPost("confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> Confirm([FromBody] ConfirmSubscriptionRequest request, CancellationToken ct = default)
    {
        var result = await newsletterService.ConfirmSubscriptionAsync(request.Token, ct);
        if (!result)
            return BadRequest(Messages.Newsletter.InvalidOrExpiredToken);
        return Success<object?>(null, Messages.Newsletter.Confirmed);
    }

    /// <summary>Désabonne un abonné de la newsletter.</summary>
    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request, CancellationToken ct = default)
    {
        var result = await newsletterService.UnsubscribeAsync(request, ct);
        if (!result)
            return NotFound(Messages.Newsletter.NotFoundOrUnsubscribed);
        return Success<object?>(null, Messages.Newsletter.Unsubscribed);
    }

    /// <summary>Supprime un abonné par son adresse email (admin).</summary>
    [HttpDelete("{email}")]
    [Authorize(Policy = "newsletter.manage")]
    public async Task<IActionResult> DeleteByEmail(string email, CancellationToken ct = default)
    {
        var result = await newsletterService.DeleteByEmailAsync(email, ct);
        if (!result)
            return NotFound(Messages.Newsletter.NotFoundOrUnsubscribed);
        return Success<object?>(null, Messages.Newsletter.Deleted);
    }

    /// <summary>Récupère la liste paginée des abonnés (admin).</summary>
    [HttpGet]
    [Authorize(Policy = "newsletter.manage")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await newsletterService.GetAllAsync(page, pageSize, ct);
        return Success(result);
    }

    /// <summary>Envoie une newsletter à tous les abonnés actifs et confirmés (admin).</summary>
    [HttpPost("send")]
    [Authorize(Policy = "newsletter.manage")]
    public async Task<IActionResult> Send([FromBody] SendNewsletterRequest request, CancellationToken ct = default)
    {
        var result = await newsletterService.SendAsync(request, ct);
        if (result.RecipientCount == 0)
            return Success(result, Messages.Newsletter.NoRecipients);
        return Success(result, Messages.Newsletter.Sent);
    }

    /// <summary>Récupère le nombre d'abonnés actifs (admin).</summary>
    [HttpGet("count")]
    [Authorize(Policy = "newsletter.manage")]
    public async Task<IActionResult> GetActiveCount(CancellationToken ct = default)
    {
        var count = await newsletterService.GetActiveCountAsync(ct);
        return Success(new { ActiveCount = count });
    }
}
