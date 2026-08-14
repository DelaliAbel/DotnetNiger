using System.Threading;
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
    public async Task<IActionResult> GetAll([FromQuery] string? partnerType, CancellationToken ct = default)
    {
        var partners = await partnerService.GetAllActiveAsync(partnerType, ct);
        return Success(partners);
    }

    /// <summary>Récupère un partenaire par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var p = await partnerService.GetByIdAsync(id, ct);
        if (p is null) return NotFound(Messages.Partner.NotFound);
        return Success(p);
    }

    /// <summary>Crée un nouveau partenaire.</summary>
    [HttpPost]
    [Authorize(Policy = "community.partners.manage")]
    public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request, CancellationToken ct = default)
    {
        var partner = await partnerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = partner.Id }, new { success = true, data = partner, message = (string?)null });
    }

    /// <summary>Met à jour un partenaire existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "community.partners.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerRequest request, CancellationToken ct = default)
    {
        var p = await partnerService.UpdateAsync(id, request, ct);
        if (p is null) return NotFound(Messages.Partner.NotFound);
        return Success(p);
    }

    /// <summary>Supprime un partenaire.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "community.partners.manage")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var deleted = await partnerService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(Messages.Partner.NotFound);
        return Success<object?>(null, Messages.Partner.Deleted);
    }
}
