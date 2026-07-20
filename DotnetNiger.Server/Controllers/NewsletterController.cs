using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController(INewsletterService newsletterService) : ControllerBase
{
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

    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        var result = await newsletterService.UnsubscribeAsync(request);
        if (!result)
            return NotFound(new { Success = false, Message = Messages.Newsletter.NotFoundOrUnsubscribed });
        return Ok(new { Success = true, Message = Messages.Newsletter.Unsubscribed });
    }

    [HttpDelete("{email}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> DeleteByEmail(string email)
    {
        var result = await newsletterService.DeleteByEmailAsync(email);
        if (!result)
            return NotFound(new { Success = false, Message = Messages.Newsletter.NotFoundOrUnsubscribed });
        return Ok(new { Success = true, Message = Messages.Newsletter.Unsubscribed });
    }

    [HttpGet]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await newsletterService.GetAllAsync(page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetActiveCount()
    {
        var count = await newsletterService.GetActiveCountAsync();
        return Ok(new { Success = true, Data = new { ActiveCount = count } });
    }
}
