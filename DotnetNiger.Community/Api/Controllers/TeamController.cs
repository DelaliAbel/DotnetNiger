using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly ITeamMemberService _teamMemberService;

    public TeamController(ITeamMemberService teamMemberService)
    {
        _teamMemberService = teamMemberService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveTeam()
    {
        var members = await _teamMemberService.GetActiveTeamMembersAsync();
        return Ok(new { data = members });
    }
}
