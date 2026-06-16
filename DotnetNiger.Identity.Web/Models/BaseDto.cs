namespace DotnetNiger.Identity.Web.Models;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
}

public class LoginHistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? Provider { get; set; }
    public string? FailureReason { get; set; }
    public string Email { get; set; } = "";
}

public class ActiveSession
{
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public DateTime LastActivity { get; set; }
    public string DeviceName { get; set; } = "";
    public string BrowserName { get; set; } = "";
}

public class DashboardStats
{
    public string TenantName { get; set; } = "—";
    public string TenantId { get; set; } = "";
    public int ActiveApiKeys { get; set; }
    public int ActiveServices { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public bool GatewayConnected { get; set; }
}

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? TenantId { get; set; }
    public List<string>? Roles { get; set; }
}

public class ProfileInput
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
}

public class TwoFactorInput
{
    public string Code { get; set; } = "";
    public string SharedKey { get; set; } = "";
    public string AuthenticatorUri { get; set; } = "";
}

public class ChangePasswordInput
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class ChangeEmailInput
{
    public string NewEmail { get; set; } = "";
}

public class ConfirmChangeEmailInput
{
    public string Code { get; set; } = "";
}

public class ApiKeyItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string KeyPrefix { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ApiKeyCreatedResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Key { get; set; } = "";
    public string KeyPrefix { get; set; } = "";
}

public class ServiceItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public bool IsActive { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminStats
{
    public int TenantCount { get; set; }
    public int UserCount { get; set; }
    public int RoleCount { get; set; }
    public int PermissionCount { get; set; }
    public int ApiKeyCount { get; set; }
    public int ServiceCount { get; set; }
    public int ClientCount { get; set; }
}

public class TenantItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AdminEmail => $"admin@{Slug}.dotnetniger.com";
}

public class CreateTenantInput
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
}

public class EditTenantInput
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class RoleItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int UserCount { get; set; }
}

public class PermissionGroup
{
    public string Category { get; set; } = "";
    public List<PermissionItem> Permissions { get; set; } = [];
}

public class PermissionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}

public class CreateRoleInput
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class EditRoleInput
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class ClientItem
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = "";
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClientInput
{
    public string ClientId { get; set; } = "";
    public string? ClientName { get; set; }
    public string? Description { get; set; }
}

public class EditClientInput
{
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class UserItem
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string>? Roles { get; set; }
}

public class CreateUserInput
{
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
}

public class EditUserInput
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsActive { get; set; }
}

public class UserChangePasswordInput
{
    public string NewPassword { get; set; } = "";
}

public class AuditLogItem
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = "";
    public Guid EntityId { get; set; }
    public string Action { get; set; } = "";
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLogPaginatedResponse
{
    public List<AuditLogItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class InviteInput
{
    public string Email { get; set; } = "";
    public string Role { get; set; } = "Admin";
}

public class PermissionGroupItem
{
    public string Category { get; set; } = "";
    public List<PermissionItem> Permissions { get; set; } = [];
}

public class RoleListResponse
{
    public List<RoleItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class CreatePermissionInput
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}

public class AssignPermissionsInput
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = [];
}

public class TwoFactorStatusResponse
{
    public bool TwoFactorEnabled { get; set; }
    public int RecoveryCodesLeft { get; set; }
}

public class TwoFactorSetupResponse
{
    public string SharedKey { get; set; } = "";
    public string AuthenticatorUri { get; set; } = "";
}

public class Enable2FAResponse
{
    public string[]? RecoveryCodes { get; set; }
}

public class PaginationModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public Func<int, string> BuildUrl { get; set; } = _ => "#";
}

public class StatCardModel
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
    public string Color { get; set; } = "primary";
}
