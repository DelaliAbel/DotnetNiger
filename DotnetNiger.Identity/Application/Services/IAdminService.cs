using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public interface IAdminService
{
    Task InviteAsync(string email, string role);
    Task<List<UserResponse>> GetAllUsersAcrossTenantsAsync();
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive);
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    Task<bool> RemoveUserRoleAsync(Guid userId, string roleName);
    Task<bool> DeleteUserAsync(Guid id);
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    Task<UserResponse?> UpdateUserProfileAsync(Guid id, UpdateUserRequest request);
    Task<UserResponse?> CreateUserAsync(AdminCreateUserRequest request);
}
