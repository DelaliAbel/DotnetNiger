using Asp.Versioning;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Community.Application;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des partenaires et sponsors de la communauté.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PartnersController(IPartnerService partnerService) : ControllerBase
{
    /// <summary>Liste les partenaires actifs, avec filtre optionnel par type.</summary>
    /// <param name="partnerType">Type de partenaire (sponsor, media...).</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? partnerType)
    {
        var partners = await partnerService.GetAllActiveAsync(partnerType);
        return Ok(new { Success = true, Data = partners });
    }

    /// <summary>Recherche un partenaire par son identifiant.</summary>
    /// <param name="id">Identifiant du partenaire.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await partnerService.GetByIdAsync(id);
        if (p is null) return NotFound(new { Success = false, Message = Messages.Partner.NotFound });
        return Ok(new { Success = true, Data = p });
    }

    /// <summary>Ajoute un nouveau partenaire (réservé aux admins).</summary>
    /// <param name="request">Informations du partenaire.</param>
    [HttpPost]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request)
    {
        var partner = await partnerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = partner.Id }, new { Success = true, Data = partner });
    }

    /// <summary>Modifie un partenaire existant (réservé aux admins).</summary>
    /// <param name="id">Identifiant du partenaire.</param>
    /// <param name="request">Nouvelles informations.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerRequest request)
    {
        var p = await partnerService.UpdateAsync(id, request);
        if (p is null) return NotFound(new { Success = false, Message = Messages.Partner.NotFound });
        return Ok(new { Success = true, Data = p });
    }

    /// <summary>Supprime un partenaire (réservé aux admins).</summary>
    /// <param name="id">Identifiant du partenaire.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await partnerService.DeleteAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Partner.NotFound });
        return Ok(new { Success = true, Message = Messages.Partner.Deleted });
    }
}
