using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

/// <summary>Contrôleur de gestion des partenaires.</summary>
[ApiController]
[Route("api/partners")]
public class PartnersController(IPartnerService partnerService) : BaseController
{
    /// <summary>Récupère tous les partenaires actifs, optionnellement filtrés par type.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? partnerType)
    {
        var partners = await partnerService.GetAllActiveAsync(partnerType);
        return Success(partners);
    }

    /// <summary>Récupère un partenaire par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await partnerService.GetByIdAsync(id);
        if (p is null) return NotFound(Messages.Partner.NotFound);
        return Success(p);
    }

    /// <summary>Crée un nouveau partenaire.</summary>
    [HttpPost]
    [Authorize(Policy = "community.partners.manage")]
    public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request)
    {
        var partner = await partnerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = partner.Id }, new { success = true, data = partner, message = (string?)null });
    }

    /// <summary>Met à jour un partenaire existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "community.partners.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerRequest request)
    {
        var p = await partnerService.UpdateAsync(id, request);
        if (p is null) return NotFound(Messages.Partner.NotFound);
        return Success(p);
    }

    /// <summary>Supprime un partenaire.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "community.partners.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await partnerService.DeleteAsync(id);
        if (!deleted) return NotFound(Messages.Partner.NotFound);
        return Success<object?>(null, Messages.Partner.Deleted);
    }
}
