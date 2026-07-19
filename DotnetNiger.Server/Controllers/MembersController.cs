using Asp.Versioning;
using DotnetNiger.Infrastructure.Services;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MembersController(IMemberDirectoryService memberService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? query, [FromQuery] string? country, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await memberService.GetAllAsync(query, country, page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("team")]
    public async Task<IActionResult> GetTeam()
    {
        var members = await memberService.GetTeamMembersAsync();
        return Ok(new { Success = true, Data = members });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await memberService.GetByIdAsync(id);
        if (member is null) return NotFound(new { Success = false, Message = "Membre non trouvé" });
        return Ok(new { Success = true, Data = member });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var userId = GetUserId();
        try
        {
            var member = await memberService.CreateProfileAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, new { Success = true, Data = member });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request)
    {
        try
        {
            var member = await memberService.UpdateProfileAsync(GetUserId(), request);
            return Ok(new { Success = true, Data = member });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Success = false, Message = "Membre non trouvé" });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await memberService.DeleteProfileAsync(GetUserId());
        if (!deleted) return NotFound(new { Success = false, Message = "Membre non trouvé" });
        return Ok(new { Success = true, Message = "Profil supprimé" });
    }
}
