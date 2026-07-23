using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de gestion de la newsletter.</summary>
[ApiController]
[Route("api/newsletter")]
public class NewsletterController(INewsletterService newsletterService) : ControllerBase
{
    /// <summary>Inscrit un abonné à la newsletter.</summary>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            var result = await newsletterService.SubscribeAsync(request);
            return Ok(new { Success = true, Data = result });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>Désabonne un abonné de la newsletter.</summary>
    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        var result = await newsletterService.UnsubscribeAsync(request);
        if (!result)
            return NotFound(new { Success = false, Message = Messages.Newsletter.NotFoundOrUnsubscribed });
        return Ok(new { Success = true, Message = Messages.Newsletter.Unsubscribed });
    }

    /// <summary>Supprime un abonné par son adresse email (admin).</summary>
    [HttpDelete("{email}")]
    [Authorize(Policy = "newsletter.manage")]
    public async Task<IActionResult> DeleteByEmail(string email)
    {
        var result = await newsletterService.DeleteByEmailAsync(email);
        if (!result)
            return NotFound(new { Success = false, Message = Messages.Newsletter.NotFoundOrUnsubscribed });
        return Ok(new { Success = true, Message = Messages.Newsletter.Unsubscribed });
    }

    /// <summary>Récupère la liste paginée des abonnés (admin).</summary>
    [HttpGet]
    [Authorize(Policy = "newsletter.manage")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await newsletterService.GetAllAsync(page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Récupère le nombre d'abonnés actifs.</summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetActiveCount()
    {
        var count = await newsletterService.GetActiveCountAsync();
        return Ok(new { Success = true, Data = new { ActiveCount = count } });
    }
}
