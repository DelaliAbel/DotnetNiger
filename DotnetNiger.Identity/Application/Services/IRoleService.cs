using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public interface IRoleService
{
    Task<RoleResponse> CreateAsync(CreateRoleRequest request);
    Task<PaginatedResponse<RoleResponse>> GetByTenantAsync(Guid tenantId, PaginationQuery pagination);
    Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request);
    Task DeleteAsync(Guid id);
    Task<RoleResponse?> GetByIdAsync(Guid id);
    Task AssignToUserAsync(Guid userId, Guid roleId);
    Task RemoveFromUserAsync(Guid userId, Guid roleId);
    Task<List<RoleResponse>> GetUserRolesAsync(Guid userId);
}
