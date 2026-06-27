using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using DotnetNiger.Identity.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Application.DTOs;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Api.Controllers;

/// <summary>Administration système : statistiques et métriques globales (Admin).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.SuperAdmin)]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;
    private readonly IdentityDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(AdminService adminService, IdentityDbContext db, UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _db = db;
        _userManager = userManager;
    }

    [HttpPost("invite")]
    public async Task<ActionResult> Invite([FromBody] InviteAdminRequest request)
    {
        await _adminService.InviteAsync(request.Email, request.Role);
        return Ok(new { message = "Invitation envoyée avec succès." });
    }

    /// <summary>Statistiques système (nombre de tenants, utilisateurs, rôles, permissions).</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var stats = await _adminService.GetSystemStatsAsync();
        return Ok(stats);
    }

    [HttpGet("tenants/{tenantId:guid}/login-history")]
    public async Task<ActionResult> GetTenantLoginHistory(Guid tenantId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetTenantLoginHistoryAsync(tenantId,
            Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
        return Ok(result);
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PaginatedResponse<AuditLog>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null, [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var query = _db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(l => l.EntityType == entityType);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<AuditLog>(items, total, page, pageSize));
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAcrossTenantsAsync();
        return Ok(users);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(new ErrorResponse("Utilisateur non trouvé"));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserResponse(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.TenantId, user.IsActive,
            user.EmailConfirmed, user.CreatedAt, roles.ToList()));
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserRequest request)
    {
        var updated = await _adminService.UpdateUserStatusAsync(id, request.IsActive ?? true);
        if (!updated)
            return NotFound(new ErrorResponse("Utilisateur non trouvé"));
        return Ok(new { message = "Statut mis à jour avec succès" });
    }

    [HttpPost("users/{id:guid}/roles")]
    public async Task<ActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleRequest request)
    {
        var assigned = await _adminService.AssignRoleToUserAsync(id, request.RoleName);
        if (!assigned)
            return BadRequest(new ErrorResponse("Impossible d'assigner le rôle"));
        return Ok(new { message = "Rôle assigné avec succès" });
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var deleted = await _adminService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse("Utilisateur non trouvé"));
        return Ok(new { message = "Utilisateur supprimé avec succès" });
    }

    [HttpPost("users")]
    public async Task<ActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
            return BadRequest(new ErrorResponse("Aucun tenant trouvé"));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName ?? ".",
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new ErrorResponse(string.Join(", ", result.Errors.Select(e => e.Description))));

        if (!string.IsNullOrEmpty(request.Role))
            await _userManager.AddToRoleAsync(user, request.Role);
        else
            await _userManager.AddToRoleAsync(user, RoleConstants.User);

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserResponse(user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.TenantId, user.IsActive, user.EmailConfirmed,
            user.CreatedAt, roles.ToList()));
    }
}
