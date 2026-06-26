using Asp.Versioning;
using DotnetNiger.Community.Application;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Annuaire des membres de la communauté DotnetNiger.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MembersController(IMemberDirectoryService memberService) : ControllerBase
{
    /// <summary>Recherche des membres avec filtres et pagination.</summary>
    /// <param name="query">Recherche textuelle (nom, bio...).</param>
    /// <param name="country">Filtre par pays.</param>
    /// <param name="page">Page demandée.</param>
    /// <param name="pageSize">Taille de la page.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? query, [FromQuery] string? country, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await memberService.GetAllAsync(query, country, page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Recherche un membre par son identifiant.</summary>
    /// <param name="id">Identifiant du membre.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await memberService.GetByIdAsync(id);
        if (member is null) return NotFound(new { Success = false, Message = Messages.Member.NotFound });
        return Ok(new { Success = true, Data = member });
    }
}
