using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetUsersAsync();
    Task<List<UserDto>> GetPendingUsersAsync();
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<UserDto>> SearchUsersAsync(string query);
    Task<List<UserDto>> GetUsersByRoleAsync(string role);
    Task<int> GetUsersCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> CreateUserAsync(CreateUserRequest user, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateUserAsync(UserDto user, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ApproveUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RejectUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetTeamMembersAsync();
    Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<bool> AddToTeamAsync(Guid userId, string position, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromTeamAsync(Guid userId, CancellationToken cancellationToken = default);
}