using System.Net.Http.Json;
using DotnetNiger.UI.Helpers;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Api;

public class ApiUserService : ApiServiceBase, IUserService
{
    public ApiUserService(HttpClient http) : base(http)
    {
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var response = await Http.GetAsync(ApiEndpoints.CommunityAdminUsers);
        if (!response.IsSuccessStatusCode)
            return [];

        return await ApiResponseReader.ReadCollectionAsync<UserDto>(response);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var response = await Http.GetAsync($"{ApiEndpoints.CommunityAdminUsers}/{userId}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await ApiResponseReader.ReadAsync<UserDto>(response);
    }

    public async Task<List<UserDto>> GetPendingUsersAsync()
    {
        var users = await GetUsersAsync();
        return users.Where(u => !u.IsActive).ToList();
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var users = await GetUsersAsync();
        return users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<UserDto>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetUsersAsync();

        var users = await GetUsersAsync();
        var q = query.Trim();
        return users.Where(u =>
            u.Username.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.Country.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.City.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.Roles.Any(r => r.Contains(q, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    public async Task<List<UserDto>> GetUsersByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return await GetUsersAsync();

        var users = await GetUsersAsync();
        return users.Where(u =>
            u.Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    public async Task<int> GetUsersCountAsync()
    {
        var users = await GetUsersAsync();
        return users.Count;
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        var users = await GetUsersAsync();
        return users.Count(u => u.IsActive);
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest user)
    {
        var content = JsonContent.Create(user);
        var response = await Http.PostAsync(ApiEndpoints.CommunityAdminUsers, content);
        if (!response.IsSuccessStatusCode)
            return null;

        return await ApiResponseReader.ReadAsync<UserDto>(response);
    }

    public async Task<UserDto?> UpdateUserAsync(UserDto user)
    {
        var existing = await GetUserByIdAsync(user.Id);
        if (existing is null) return null;

        var statusChanged = existing.IsActive != user.IsActive;
        if (statusChanged)
        {
            var statusContent = JsonContent.Create(new UpdateUserStatusRequest { IsActive = user.IsActive });
            var statusResponse = await Http.PatchAsync($"{ApiEndpoints.AdminUsers}/{user.Id}/status", statusContent);
            if (!statusResponse.IsSuccessStatusCode)
                return null;
        }

        var teamChanged = existing.IsTeamMember != user.IsTeamMember || existing.Position != user.Position;
        if (teamChanged)
        {
            var teamContent = JsonContent.Create(new UpdateTeamRequest { IsTeamMember = user.IsTeamMember, Position = user.Position });
            var teamResponse = await Http.PatchAsync(
                string.Format(ApiEndpoints.AdminUserTeam, user.Id), teamContent);
            if (!teamResponse.IsSuccessStatusCode)
                return null;
        }

        var rolesChanged = existing.Roles.Count != user.Roles.Count ||
            !existing.Roles.OrderBy(r => r).SequenceEqual(user.Roles.OrderBy(r => r));
        if (rolesChanged)
        {
            var rolesToRemove = existing.Roles.Except(user.Roles).ToList();
            var rolesToAdd = user.Roles.Except(existing.Roles).ToList();

            foreach (var role in rolesToRemove)
            {
                var response = await Http.DeleteAsync(
                    string.Format(ApiEndpoints.AdminUserRole, user.Id, role));
                if (!response.IsSuccessStatusCode)
                    return null;
            }
            foreach (var role in rolesToAdd)
            {
                var content = JsonContent.Create(new UpdateUserRolesRequest { RoleName = role });
                var response = await Http.PostAsync(
                    string.Format(ApiEndpoints.AdminUserRoles, user.Id), content);
                if (!response.IsSuccessStatusCode)
                    return null;
            }
        }

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var response = await Http.DeleteAsync($"{ApiEndpoints.CommunityAdminUsers}/{userId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveUserAsync(Guid userId)
    {
        var statusContent = JsonContent.Create(new UpdateUserStatusRequest { IsActive = true });
        var response = await Http.PatchAsync($"{ApiEndpoints.AdminUsers}/{userId}/status", statusContent);
        if (!response.IsSuccessStatusCode)
            return false;

        var roleContent = JsonContent.Create(new UpdateUserRolesRequest { RoleName = "Collaborator" });
        await Http.PostAsync(string.Format(ApiEndpoints.AdminUserRoles, userId), roleContent);

        return true;
    }

    public async Task<bool> RejectUserAsync(Guid userId)
    {
        var content = JsonContent.Create(new UpdateUserStatusRequest { IsActive = false });
        var response = await Http.PatchAsync($"{ApiEndpoints.AdminUsers}/{userId}/status", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<UserDto>> GetTeamMembersAsync()
    {
        var users = await GetUsersAsync();
        return users.Where(u => u.Roles.Any(r =>
            r.Equals("Collaborator", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName)
    {
        var content = JsonContent.Create(new UpdateUserRolesRequest { RoleName = roleName });
        var response = await Http.PostAsync(
            string.Format(ApiEndpoints.AdminUserRoles, userId), content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddToTeamAsync(Guid userId, string position)
    {
        var content = JsonContent.Create(new UpdateTeamRequest
        {
            IsTeamMember = true,
            Position = position
        });
        var response = await Http.PatchAsync(
            string.Format(ApiEndpoints.AdminUserTeam, userId), content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveFromTeamAsync(Guid userId)
    {
        var content = JsonContent.Create(new UpdateTeamRequest
        {
            IsTeamMember = false,
            Position = string.Empty
        });
        var response = await Http.PatchAsync(
            string.Format(ApiEndpoints.AdminUserTeam, userId), content);
        return response.IsSuccessStatusCode;
    }
}
