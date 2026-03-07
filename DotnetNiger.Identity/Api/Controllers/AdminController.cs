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
[Authorize(Roles = "Admin")]
// Endpoints d'administration et supervision des utilisateurs.
public class AdminController : ControllerBase
{
	private readonly IAdminService _adminService;
	private readonly IUserService _userService;
	private readonly ILoginHistoryService _loginHistoryService;
	private readonly IRoleService _roleService;
	private readonly IPermissionService _permissionService;
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
		_tokenService = tokenService;
		_userManager = userManager;
		_roleManager = roleManager;
		_dbContext = dbContext;
	}

	[HttpGet("users")]
	public async Task<ActionResult<PaginatedDto<UserSummaryDto>>> GetUsers(
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
		if (skip < 0)
		{
			skip = 0;
		}

		if (take <= 0)
		{
			take = 20;
		}

		if (take > 100)
		{
			take = 100;
		}

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
		return Ok(result);
	}

	[HttpGet("users/{userId:guid}")]
	public async Task<ActionResult<UserDto>> GetUser(Guid userId)
	{
		var profile = await _userService.GetProfileAsync(userId);
		return Ok(profile);
	}

    [HttpDelete("users/{userId:guid}")]
	public async Task<IActionResult> DeleteUser(Guid userId)
	{
		await _adminService.DeleteUserAsync(userId);
		return NoContent();
	}

	[HttpPut("users/{userId:guid}/status")]
	public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
	{
		await _adminService.SetUserActiveAsync(userId, request.IsActive);
		return NoContent();
	}


	[HttpGet("users/{userId:guid}/login-history")]
	public async Task<ActionResult<PaginatedDto<LoginHistoryDto>>> GetUserLoginHistory(
		Guid userId,
		[FromQuery] int skip = 0,
		[FromQuery] int take = 20)
	{
		if (skip < 0)
		{
			skip = 0;
		}

		if (take <= 0)
		{
			take = 20;
		}

		if (take > 100)
		{
			take = 100;
		}

		var result = await _loginHistoryService.GetUserHistoryAsync(userId, skip, take);
		return Ok(result);
	}

	[HttpGet("audit-logs")]
	public async Task<ActionResult<PaginatedDto<AdminAuditLogDto>>> GetAuditLogs(
		[FromQuery] Guid? adminUserId,
		[FromQuery] string? action,
		[FromQuery] string? targetType,
		[FromQuery] DateTime? createdFrom,
		[FromQuery] DateTime? createdTo,
		[FromQuery] int skip = 0,
		[FromQuery] int take = 20)
	{
		// Filtrage simple pour retrouver les actions sensibles.
		if (skip < 0)
		{
			skip = 0;
		}

		if (take <= 0)
		{
			take = 20;
		}

		if (take > 100)
		{
			take = 100;
		}

		var result = await _adminService.GetAdminAuditLogsAsync(
			adminUserId,
			action,
			targetType,
			createdFrom,
			createdTo,
			skip,
			take);
		return Ok(result);
	}

	// --- Settings endpoints ---

	[HttpGet("settings/file-upload")]
	public async Task<ActionResult<FileUploadSettingsDto>> GetFileUploadSettings()
	{
		var settings = await _adminService.GetFileUploadSettingsAsync();
		return Ok(settings);
	}

	[HttpPut("settings/file-upload")]
	public async Task<ActionResult<FileUploadSettingsDto>> UpdateFileUploadSettings(
		[FromBody] UpdateFileUploadSettingsRequest request)
	{
		var settings = await _adminService.UpdateFileUploadSettingsAsync(request);
		return Ok(settings);
	}

	// --- User Management endpoints ---

	[HttpPost("users")]
	public async Task<ActionResult<UserDto>> CreateUser([FromBody] AdminCreateUserRequest request)
	{
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
			return BadRequest(new { errors = result.Errors });
		}

		var profile = await _userService.GetProfileAsync(user.Id);
		return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, profile);
	}

	[HttpPut("users/{userId:guid}")]
	public async Task<ActionResult<UserDto>> UpdateUser(Guid userId, [FromBody] AdminUpdateUserRequest request)
	{
		var user = await _userManager.FindByIdAsync(userId.ToString());
		if (user == null)
		{
			return NotFound(new { message = "User not found." });
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
			return BadRequest(new { errors = result.Errors });
		}

		var profile = await _userService.GetProfileAsync(userId);
		return Ok(profile);
	}

	[HttpPost("users/{userId:guid}/reset-password")]
	public async Task<IActionResult> ResetUserPassword(Guid userId, [FromBody] AdminResetPasswordRequest request)
	{
		var user = await _userManager.FindByIdAsync(userId.ToString());
		if (user == null)
		{
			return NotFound(new { message = "User not found." });
		}

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

		if (!result.Succeeded)
		{
			return BadRequest(new { errors = result.Errors });
		}

		return Ok(new { message = "Password reset successfully." });
	}

	[HttpPost("users/{userId:guid}/force-logout")]
	public async Task<IActionResult> ForceLogoutUser(Guid userId)
	{
		var tokens = await _dbContext.RefreshTokens
			.Where(t => t.UserId == userId && t.RevokedAt == null)
			.ToListAsync();

		foreach (var token in tokens)
		{
			token.RevokedAt = DateTime.UtcNow;
		}

		if (tokens.Any())
		{
			await _dbContext.SaveChangesAsync();
		}

		return NoContent();
	}

	[HttpPost("users/{userId:guid}/unlock")]
	public async Task<IActionResult> UnlockUser(Guid userId)
	{
		var user = await _userManager.FindByIdAsync(userId.ToString());
		if (user == null)
		{
			return NotFound(new { message = "User not found." });
		}

		var result = await _userManager.SetLockoutEndDateAsync(user, null);
		if (!result.Succeeded)
		{
			return BadRequest(new { errors = result.Errors });
		}

		return NoContent();
	}

	// --- Roles Management endpoints ---

	[HttpPost("roles")]
	public async Task<ActionResult<RoleDto>> CreateRole([FromBody] AddRoleRequest request)
	{
		var role = await _roleService.CreateAsync(request);
		return CreatedAtAction(nameof(GetRole), new { roleId = role.Id }, role);
	}

	[HttpGet("roles")]
	public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles()
	{
		var roles = await _roleService.GetAllAsync();
		return Ok(roles);
	}

	[HttpGet("roles/{roleId:guid}")]
	public async Task<ActionResult<RoleDto>> GetRole(Guid roleId)
	{
		var role = await _dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
		if (role == null)
		{
			return NotFound(new { message = "Role not found." });
		}

		return Ok(new RoleDto { Id = role.Id, Name = role.Name });
	}

	[HttpPut("roles/{roleId:guid}")]
	public async Task<ActionResult<RoleDto>> UpdateRole(Guid roleId, [FromBody] UpdateRoleRequest request)
	{
		var role = await _roleManager.FindByIdAsync(roleId.ToString());
		if (role == null)
		{
			return NotFound(new { message = "Role not found." });
		}

		role.Name = request.Name ?? role.Name;
		role.NormalizedName = request.Name?.ToUpperInvariant() ?? role.NormalizedName;

		var result = await _roleManager.UpdateAsync(role);
		if (!result.Succeeded)
		{
			return BadRequest(new { errors = result.Errors });
		}

		return Ok(new RoleDto { Id = role.Id, Name = role.Name });
	}

	[HttpDelete("roles/{roleId:guid}")]
	public async Task<IActionResult> DeleteRole(Guid roleId)
	{
		await _roleService.DeleteAsync(roleId);
		return NoContent();
	}

	[HttpPost("users/{userId:guid}/roles")]
	public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleRequest request)
	{
		await _roleService.AssignToUserAsync(new AssignRoleRequest { UserId = userId, RoleName = request.RoleName });
		return NoContent();
	}

	[HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
	public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
	{
		var role = await _roleManager.FindByIdAsync(roleId.ToString());
		if (role == null)
		{
			return NotFound(new { message = "Role not found." });
		}

		await _roleService.RemoveFromUserAsync(new AssignRoleRequest { UserId = userId, RoleName = role.Name! });
		return NoContent();
	}

	[HttpGet("roles/{roleId:guid}/permissions")]
	public async Task<ActionResult<List<PermissionDto>>> GetRolePermissions(Guid roleId)
	{
		var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
		return Ok(permissions);
	}

	[HttpPut("roles/{roleId:guid}/permissions")]
	public async Task<IActionResult> SetRolePermissions(Guid roleId, [FromBody] SetRolePermissionsRequest request)
	{
		var role = await _roleManager.FindByIdAsync(roleId.ToString());
		if (role == null)
		{
			return NotFound(new { message = "Role not found." });
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
		return NoContent();
	}

	// --- Audit endpoints ---

	[HttpGet("audit/logs")]
	public async Task<ActionResult<PaginatedDto<AdminAuditLogDto>>> GetAllAuditLogs(
		[FromQuery] int skip = 0,
		[FromQuery] int take = 20)
	{
		var result = await _adminService.GetAdminAuditLogsAsync(null, null, null, null, null, skip, take);
		return Ok(result);
	}

	[HttpGet("audit/logins")]
	public async Task<ActionResult<PaginatedDto<LoginHistoryDto>>> GetLoginLogs(
		[FromQuery] int skip = 0,
		[FromQuery] int take = 20)
	{
		var items = await _dbContext.LoginHistories
			.AsNoTracking()
			.OrderByDescending(l => l.LoginAt)
			.Skip(skip)
			.Take(take)
			.Select(l => new LoginHistoryDto
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

		return Ok(new PaginatedDto<LoginHistoryDto>
		{
			Items = items,
			TotalCount = total,
			Skip = skip,
			Take = take
		});
	}

	[HttpGet("audit/admin-actions")]
	public async Task<ActionResult<PaginatedDto<AdminAuditLogDto>>> GetAdminActionLogs(
		[FromQuery] int skip = 0,
		[FromQuery] int take = 20)
	{
		var result = await _adminService.GetAdminAuditLogsAsync(null, null, null, null, null, skip, take);
		return Ok(result);
	}

	[HttpPut("audit/retention-policy")]
	public Task<IActionResult> SetRetentionPolicy([FromBody] AuditRetentionPolicyRequest request)
	{
		// For now, just acknowledge the policy (can be stored in config or database later)
		return Task.FromResult<IActionResult>(Ok(new { message = "Retention policy updated.", retentionDays = request.RetentionDays }));
	}
}
