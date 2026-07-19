using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Domain.Interfaces;

public interface IIdentityApiClient
{
    Task<List<CommunityUserResponse>> GetUsersAsync();
    Task<CommunityUserResponse?> GetUserAsync(Guid id);
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive);
    Task<string?> RegisterUserAsync(string email, string password, string fullName, string? role = null);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    Task<bool> RemoveUserRoleAsync(Guid userId, string roleName);
    Task<bool> ReplaceUserRolesAsync(Guid userId, string newRole);
    Task<bool> UpdateUserProfileAsync(Guid id, string? firstName, string? lastName, string? avatarUrl);
}
