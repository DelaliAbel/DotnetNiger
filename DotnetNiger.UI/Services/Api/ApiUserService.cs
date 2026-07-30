using System.Net.Http.Json;
using DotnetNiger.UI.Helpers;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public class ApiUserService : ApiServiceBase, IUserService
{
    public ApiUserService(HttpClient http, ILogger<ApiUserService> logger) : base(http, logger) { }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var url = ApiEndpoints.AdminUsers;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<UserDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.AdminUsers}/{userId}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<UserDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<List<UserDto>> GetPendingUsersAsync()
    {
        var users = await GetUsersAsync();
        return users.Where(u => !u.IsActive).ToList();
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
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

    public async Task<int> GetUsersCountAsync(CancellationToken cancellationToken = default)
    {
        var users = await GetUsersAsync();
        return users.Count;
    }

    public async Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default)
    {
        var users = await GetUsersAsync();
        return users.Count(u => u.IsActive);
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest user, CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.AdminUsers;
        try
        {
            var content = JsonContent.Create(user);
            var response = await Http.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<UserDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return null;
        }
    }

    public async Task<UserDto?> UpdateUserAsync(UserDto user, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await GetUserByIdAsync(user.Id);
            if (existing is null) return null;

            var statusChanged = existing.IsActive != user.IsActive;
            if (statusChanged)
            {
                var statusContent = JsonContent.Create(new UpdateUserStatusRequest { IsActive = user.IsActive });
                var statusResponse = await Http.PatchAsync($"{ApiEndpoints.AdminUsers}/{user.Id}/status", statusContent);
                if (!statusResponse.IsSuccessStatusCode)
                {
                    Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)statusResponse.StatusCode, $"{ApiEndpoints.AdminUsers}/{user.Id}/status");
                    return null;
                }
            }

            var teamChanged = existing.IsTeamMember != user.IsTeamMember || existing.Position != user.Position;
            if (teamChanged)
            {
                var teamContent = JsonContent.Create(new UpdateTeamRequest { IsTeamMember = user.IsTeamMember, Position = user.Position });
                var teamUrl = string.Format(ApiEndpoints.AdminUserTeam, user.Id);
                var teamResponse = await Http.PatchAsync(teamUrl, teamContent);
                if (!teamResponse.IsSuccessStatusCode)
                {
                    Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)teamResponse.StatusCode, teamUrl);
                    return null;
                }
            }

            var rolesChanged = existing.Roles.Count != user.Roles.Count ||
                !existing.Roles.OrderBy(r => r).SequenceEqual(user.Roles.OrderBy(r => r));
            if (rolesChanged)
            {
                var rolesToRemove = existing.Roles.Except(user.Roles).ToList();
                var rolesToAdd = user.Roles.Except(existing.Roles).ToList();

                foreach (var role in rolesToRemove)
                {
                    var roleUrl = string.Format(ApiEndpoints.AdminUserRole, user.Id, role);
                    var response = await Http.DeleteAsync(roleUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.LogWarning("Failed {StatusCode} on DELETE {Url}", (int)response.StatusCode, roleUrl);
                        return null;
                    }
                }
                foreach (var role in rolesToAdd)
                {
                    var roleUrl = string.Format(ApiEndpoints.AdminUserRoles, user.Id);
                    var roleContent = JsonContent.Create(new UpdateUserRolesRequest { RoleName = role });
                    var response = await Http.PostAsync(roleUrl, roleContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, roleUrl);
                        return null;
                    }
                }
            }

            return await GetUserByIdAsync(user.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating user {UserId}", user.Id);
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.AdminUsers}/{userId}";
        try
        {
            var response = await Http.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on DELETE {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on DELETE {Url}", url);
            return false;
        }
    }

    public async Task<bool> ApproveUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var statusUrl = $"{ApiEndpoints.AdminUsers}/{userId}/status";
        var roleUrl = string.Format(ApiEndpoints.AdminUserRoles, userId);
        try
        {
            var statusContent = JsonContent.Create(new UpdateUserStatusRequest { IsActive = true });
            var response = await Http.PatchAsync(statusUrl, statusContent);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, statusUrl);
                return false;
            }

            var roleContent = JsonContent.Create(new UpdateUserRolesRequest { RoleName = "Collaborator" });
            await Http.PostAsync(roleUrl, roleContent);

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error approving user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> RejectUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.AdminUsers}/{userId}/status";
        try
        {
            var content = JsonContent.Create(new UpdateUserStatusRequest { IsActive = false });
            var response = await Http.PatchAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rejecting user {UserId}", userId);
            return false;
        }
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

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var url = string.Format(ApiEndpoints.AdminUserRoles, userId);
        try
        {
            var content = JsonContent.Create(new UpdateUserRolesRequest { RoleName = roleName });
            var response = await Http.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }

    public async Task<bool> AddToTeamAsync(Guid userId, string position, CancellationToken cancellationToken = default)
    {
        var url = string.Format(ApiEndpoints.AdminUserTeam, userId);
        try
        {
            var content = JsonContent.Create(new UpdateTeamRequest
            {
                IsTeamMember = true,
                Position = position
            });
            var response = await Http.PatchAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return false;
        }
    }

    public async Task<bool> RemoveFromTeamAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var url = string.Format(ApiEndpoints.AdminUserTeam, userId);
        try
        {
            var content = JsonContent.Create(new UpdateTeamRequest
            {
                IsTeamMember = false,
                Position = string.Empty
            });
            var response = await Http.PatchAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return false;
        }
    }
}
