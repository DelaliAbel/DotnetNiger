using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

[ApiController]
[Route("api/stats")]
public class StatsController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var dashboard = await adminService.GetDashboardAsync();
        return Ok(new { Success = true, Data = dashboard });
    }
}
