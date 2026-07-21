using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Admin;

public interface IAdminService
{
    Task InviteAsync(string email, string role);
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<List<UserResponse>> GetUsersAsync();
    Task<UserResponse?> GetUserAsync(Guid id);
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive);
    Task<bool> UpdateUserTeamAsync(Guid id, bool isTeamMember, string position);
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    Task<bool> ReplaceUserRolesAsync(Guid userId, string roleName);
    Task<bool> RemoveUserRoleAsync(Guid userId, string roleName);
    Task<bool> DeleteUserAsync(Guid id);
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    Task<UserResponse?> UpdateUserProfileAsync(Guid id, UpdateUserRequest request);
    Task<UserResponse?> CreateUserAsync(AdminCreateUserRequest request);
    Task<UserResponse?> CreateUserAsync(CreateAdminUserRequest request);
    Task<DashboardStats> GetDashboardAsync();
}
