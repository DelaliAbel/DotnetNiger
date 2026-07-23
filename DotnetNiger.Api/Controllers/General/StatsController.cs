using System.Security.Claims;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Api.Entities;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de statistiques du tableau de bord.</summary>
[ApiController]
[Route("api/stats")]
[Authorize]
public class StatsController(
    IAdminService adminService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    /// <summary>Récupère les statistiques du tableau de bord selon le rôle de l'utilisateur.</summary>
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null) return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);

        if (roles.Any(r => r == RoleConstants.SuperAdmin || r == RoleConstants.Admin))
        {
            var dashboard = await adminService.GetDashboardAsync();
            return Ok(new { Success = true, Data = dashboard });
        }

        if (roles.Contains(RoleConstants.Collaborator))
        {
            var dashboard = await adminService.GetCollaboratorDashboardAsync(userId.Value);
            return Ok(new { Success = true, Data = dashboard });
        }

        return Ok(new { Success = true, Data = new { } });
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
