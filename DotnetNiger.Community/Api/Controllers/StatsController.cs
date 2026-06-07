using Asp.Versioning;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class StatsController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var dashboard = await adminService.GetDashboardAsync();
        return Ok(new { Success = true, Data = dashboard });
    }
}
