// Controleur API Identity: AdminController
using Asp.Versioning;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "SuperAdmin")]
// Endpoints d'administration et supervision des utilisateurs.
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;
    private readonly ILoginHistoryService _loginHistoryService;
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly IAccountDeletionService _accountDeletionService;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly DotnetNigerIdentityDbContext _dbContext;

    public AdminController(
        IAdminService adminService,
        IUserService userService,
        ILoginHistoryService loginHistoryService,
        IRoleService roleService,
        IPermissionService permissionService,
        IAccountDeletionService accountDeletionService,
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        DotnetNigerIdentityDbContext dbContext)
    {
        _adminService = adminService;
        _userService = userService;
        _loginHistoryService = loginHistoryService;
        _roleService = roleService;
        _permissionService = permissionService;
        _accountDeletionService = accountDeletionService;
        _tokenService = tokenService;
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool? emailConfirmed,
        [FromQuery] string? role,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        (skip, take) = NormalizePaging(skip, take);

        var result = await _adminService.GetUsersAsync(
            search,
            isActive,
            emailConfirmed,
            role,
            createdFrom,
            createdTo,
            sortBy,
            sortDirection,
            skip,
            take);
        return Success(result);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var profile = await _userService.GetProfileAsync(userId);
        return Success(profile);
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await _adminService.DeleteUserAsync(userId);
        return SuccessMessage("User deleted successfully.");
    }

    [HttpPut("users/{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
    {
        await _adminService.SetUserActiveAsync(userId, request.IsActive);
        return SuccessMessage("User status updated successfully.");
    }


    [HttpGet("users/{userId:guid}/login-history")]
    public async Task<IActionResult> GetUserLoginHistory(
        Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        (skip, take) = NormalizePaging(skip, take);

        var result = await _loginHistoryService.GetUserHistoryAsync(userId, skip, take);
        return Success(result);
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? adminUserId,
        [FromQuery] string? action,
        [FromQuery] string? targetType,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        (skip, take) = NormalizePaging(skip, take);

        var result = await _adminService.GetAdminAuditLogsAsync(
            adminUserId,
            action,
            targetType,
            createdFrom,
            createdTo,
            skip,
            take);
        return Success(result);
    }

    // --- Settings endpoints ---

    [HttpGet("settings/file-upload")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetFileUploadSettings()
    {
        var settings = await _adminService.GetFileUploadSettingsAsync();
        return Success(settings);
    }

    [HttpPut("settings/file-upload")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateFileUploadSettings(
        [FromBody] UpdateFileUploadSettingsRequest request)
    {
        var settings = await _adminService.UpdateFileUploadSettingsAsync(request);
        return Success(settings, "File upload settings updated successfully.");
    }

    [HttpGet("settings/features")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetFeatureSettings()
    {
        var settings = await _adminService.GetFeatureSettingsAsync();
        return Success(settings);
    }

    [HttpPut("settings/features")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateFeatureSettings([FromBody] UpdateFeatureSettingsRequest request)
    {
        var settings = await _adminService.UpdateFeatureSettingsAsync(request);
        return Success(settings, "Feature settings updated successfully.");
    }

    [HttpGet("settings/account-deletion")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAccountDeletionSettings()
    {
        var settings = await _adminService.GetAccountDeletionSettingsAsync();
        return Success(settings);
    }

    [HttpPut("settings/account-deletion")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateAccountDeletionSettings([FromBody] UpdateAccountDeletionSettingsRequest request)
    {
        var reviewerId = RequireAuthenticatedUserId();
        var settings = await _adminService.UpdateAccountDeletionSettingsAsync(request, reviewerId);
        return Success(settings, "Account deletion settings updated successfully.");
    }

    [HttpGet("settings/auth")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAuthSettings()
    {
        var settings = await _adminService.GetAuthSettingsAsync();
        return Success(settings);
    }

    [HttpPut("settings/auth")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateAuthSettings([FromBody] UpdateAuthSettingsRequest request)
    {
        var reviewerId = RequireAuthenticatedUserId();
        var settings = await _adminService.UpdateAuthSettingsAsync(request, reviewerId);
        return Success(settings, "Auth settings updated successfully.");
    }

    [HttpGet("settings/oauth")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetOAuthProviderSettings()
    {
        var settings = await _adminService.GetOAuthProviderSettingsAsync();
        return Success(settings);
    }

    [HttpPut("settings/oauth/{provider}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateOAuthProviderSettings(string provider, [FromBody] UpdateOAuthProviderSettingsRequest request)
    {
        var reviewerId = RequireAuthenticatedUserId();
        var settings = await _adminService.UpdateOAuthProviderSettingsAsync(provider, request, reviewerId);
        return Success(settings, "OAuth provider settings updated successfully.");
    }

    // --- User Management endpoints ---

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
    {
        var roleName = NormalizeRoleName(request.RoleName);
        if (roleName is null)
        {
            return BadRequestProblem("Role must be one of: Member, Admin, SuperAdmin.");
        }

        if ((roleName == "Admin" || roleName == "SuperAdmin") && !User.IsInRole("SuperAdmin"))
        {
            return Forbid();
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = (request.FirstName + " " + request.LastName).Trim(),
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = request.EmailConfirmed ?? false,
            IsActive = request.IsActive ?? true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            }));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["identity"] = roleResult.Errors.Select(error => error.Description).ToArray()
            }));
        }

        var profile = await _userService.GetProfileAsync(user.Id);
        return CreatedSuccess(nameof(GetUser), new { userId = user.Id }, profile, "User created successfully.");
    }

    [HttpPut("users/{userId:guid}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] AdminUpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFoundProblem("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName))
        {
            var firstName = request.FirstName ?? user.FullName.Split(' ').FirstOrDefault() ?? string.Empty;
            var lastName = request.LastName ?? string.Join(" ", user.FullName.Split(' ').Skip(1));
            user.FullName = (firstName + " " + lastName).Trim();
        }
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.IsActive = request.IsActive ?? user.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            }));
        }

        var profile = await _userService.GetProfileAsync(userId);
        return Success(profile, "User updated successfully.");
    }

    [HttpPost("users/{userId:guid}/reset-password")]
    public async Task<IActionResult> ResetUserPassword(Guid userId, [FromBody] AdminResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFoundProblem("User not found.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            }));
        }

        return SuccessMessage("Password reset successfully.");
    }

    [HttpPost("users/{userId:guid}/force-logout")]
    public async Task<IActionResult> ForceLogoutUser(Guid userId)
    {
        var revoked = await _adminService.ForceLogoutUserSessionsAsync(userId);
        return Success(new { revokedSessions = revoked }, "User sessions revoked successfully.");
    }

    [HttpPost("users/{userId:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFoundProblem("User not found.");
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            }));
        }

        return SuccessMessage("User unlocked successfully.");
    }

    [HttpPost("users/{userId:guid}/lock")]
    public async Task<IActionResult> LockUser(Guid userId, [FromQuery] int hours = 24)
    {
        if (hours < 1 || hours > 24 * 365)
        {
            return BadRequestProblem("Lock duration must be between 1 and 8760 hours.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFoundProblem("User not found.");
        }

        user.LockoutEnabled = true;
        var lockoutEnd = DateTimeOffset.UtcNow.AddHours(hours);
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            }));
        }

        return Success(new
        {
            userId = user.Id,
            lockedUntilUtc = lockoutEnd
        }, "User locked successfully.");
    }

    [HttpGet("account-deletion-requests")]
    public async Task<IActionResult> GetAccountDeletionRequests([FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        (skip, take) = NormalizePaging(skip, take);
        var requests = await _accountDeletionService.GetPendingAsync(skip, take);
        return Success(requests);
    }

    [HttpPost("account-deletion-requests/{requestId:guid}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveAccountDeletion(Guid requestId)
    {
        var reviewerId = RequireAuthenticatedUserId();
        await _accountDeletionService.ApproveAsync(requestId, reviewerId);
        return SuccessMessage("Account deletion request approved.");
    }

    [HttpPost("account-deletion-requests/{requestId:guid}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectAccountDeletion(Guid requestId, [FromBody] RejectAccountDeletionRequest request)
    {
        var reviewerId = RequireAuthenticatedUserId();
        await _accountDeletionService.RejectAsync(requestId, reviewerId, request.Reason);
        return SuccessMessage("Account deletion request rejected.");
    }

    [HttpPost("account-deletions/execute")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExecuteApprovedAccountDeletions([FromQuery] int batchSize = 100)
    {
        var reviewerId = RequireAuthenticatedUserId();
        var executedCount = await _accountDeletionService.ExecuteApprovedAsync(reviewerId, batchSize);
        return Success(new { executedCount }, "Approved account deletions executed.");
    }

    // --- Roles Management endpoints ---

    [HttpPost("roles")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateRole([FromBody] AddRoleRequest request)
    {
        var roleName = NormalizeRoleName(request.Name);
        if (roleName is null)
        {
            return BadRequestProblem("Role must be one of: Member, Admin, SuperAdmin.");
        }

        request.Name = roleName;
        var role = await _roleService.CreateAsync(request);
        return CreatedSuccess(nameof(GetRole), new { roleId = role.Id }, role, "Role created successfully.");
    }

    [HttpGet("roles")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleService.GetAllAsync();
        return Success(roles);
    }

    [HttpGet("roles/{roleId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetRole(Guid roleId)
    {
        var role = await _dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null)
        {
            return NotFoundProblem("Role not found.");
        }

        return Success(new RoleResponse { Id = role.Id, Name = role.Name ?? string.Empty });
    }

    [HttpPut("roles/{roleId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] UpdateRoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return NotFoundProblem("Role not found.");
        }

        role.Name = request.Name ?? role.Name;
        role.NormalizedName = request.Name?.ToUpperInvariant() ?? role.NormalizedName;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            }));
        }

        return Success(new RoleResponse { Id = role.Id, Name = role.Name ?? string.Empty }, "Role updated successfully.");
    }

    [HttpDelete("roles/{roleId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        await _roleService.DeleteAsync(roleId);
        return SuccessMessage("Role deleted successfully.");
    }

    [HttpPost("users/{userId:guid}/roles")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleRequest request)
    {
        var roleName = NormalizeRoleName(request.RoleName);
        if (roleName is null)
        {
            return BadRequestProblem("Role must be one of: Member, Admin, SuperAdmin.");
        }

        request.RoleName = roleName;
        await _roleService.AssignToUserAsync(new AssignRoleRequest { UserId = userId, RoleName = request.RoleName });
        return SuccessMessage("Role assigned successfully.");
    }

    [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return NotFoundProblem("Role not found.");
        }

        await _roleService.RemoveFromUserAsync(new AssignRoleRequest { UserId = userId, RoleName = role.Name! });
        return SuccessMessage("Role removed successfully.");
    }

    [HttpGet("roles/{roleId:guid}/permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetRolePermissions(Guid roleId)
    {
        var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
        return Success(permissions);
    }

    [HttpPut("roles/{roleId:guid}/permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SetRolePermissions(Guid roleId, [FromBody] SetRolePermissionsRequest request)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return NotFoundProblem("Role not found.");
        }

        // Remove existing permissions
        var existingPerms = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        _dbContext.RolePermissions.RemoveRange(existingPerms);

        // Add new permissions
        foreach (var permId in request.PermissionIds)
        {
            _dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permId
            });
        }

        await _dbContext.SaveChangesAsync();
        return SuccessMessage("Role permissions updated successfully.");
    }

    private static string? NormalizeRoleName(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return "Member";
        }

        return roleName.Trim().ToLowerInvariant() switch
        {
            "member" => "Member",
            "admin" => "Admin",
            "superadmin" => "SuperAdmin",
            "super-admin" => "SuperAdmin",
            _ => null
        };
    }

    // --- Audit endpoints ---

    [HttpGet("audit/logs")]
    public async Task<IActionResult> GetAllAuditLogs(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var result = await _adminService.GetAdminAuditLogsAsync(null, null, null, null, null, skip, take);
        return Success(result);
    }

    [HttpGet("audit/logins")]
    public async Task<IActionResult> GetLoginLogs(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var items = await _dbContext.LoginHistories
            .AsNoTracking()
            .OrderByDescending(l => l.LoginAt)
            .Skip(skip)
            .Take(take)
            .Select(l => new LoginHistoryResponse
            {
                Id = l.Id,
                LoginAt = l.LoginAt,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                Success = l.Success,
                FailureReason = l.FailureReason,
                Country = l.Country,
                City = l.City
            })
            .ToListAsync();

        var total = await _dbContext.LoginHistories.CountAsync();

        return Success(new PaginatedResponse<LoginHistoryResponse>
        {
            Items = items,
            TotalCount = total,
            Skip = skip,
            Take = take
        });
    }

    [HttpGet("audit/admin-actions")]
    public async Task<IActionResult> GetAdminActionLogs(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var result = await _adminService.GetAdminAuditLogsAsync(null, null, null, null, null, skip, take);
        return Success(result);
    }

    [HttpPut("audit/retention-policy")]
    public Task<IActionResult> SetRetentionPolicy([FromBody] AuditRetentionPolicyRequest request)
    {
        // For now, just acknowledge the policy (can be stored in config or database later)
        return Task.FromResult<IActionResult>(Success(new { retentionDays = request.RetentionDays }, "Retention policy updated."));
    }
}
