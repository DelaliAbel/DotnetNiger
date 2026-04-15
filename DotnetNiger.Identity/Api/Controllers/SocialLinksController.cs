// Controleur API Identity: SocialLinksController
using Asp.Versioning;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/social-links")]
[Authorize]
// Endpoints pour les liens sociaux de l'utilisateur connecte.
public class SocialLinksController : ApiControllerBase
{
    // Endpoints proteges pour les liens sociaux.
    private readonly ISocialLinkService _socialLinkService;

    public SocialLinksController(ISocialLinkService socialLinkService)
    {
        _socialLinkService = socialLinkService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyLinks()
    {
        var userId = RequireAuthenticatedUserId();
        var links = await _socialLinkService.GetForUserAsync(userId);
        return Success(links);
    }

    [HttpPost]
    public async Task<IActionResult> AddLink([FromBody] AddSocialLinkRequest request)
    {
        var userId = RequireAuthenticatedUserId();
        var link = await _socialLinkService.AddAsync(userId, request);
        return Success(link, "Social link added successfully.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLink(Guid id)
    {
        var userId = RequireAuthenticatedUserId();
        await _socialLinkService.DeleteAsync(userId, id);
        return SuccessMessage("Social link deleted successfully.");
    }
}
