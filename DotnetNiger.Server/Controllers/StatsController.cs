using Asp.Versioning;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class StatsController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var dashboard = await adminService.GetDashboardAsync();
        return Ok(new { Success = true, Data = dashboard });
    }
}
