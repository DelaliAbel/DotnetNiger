using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface IRoleService
{
    Task<RoleResponse> CreateAsync(CreateRoleRequest request);
    Task<PaginatedResponse<RoleResponse>> GetAllAsync(PaginationQuery pagination);
    Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request);
    Task DeleteAsync(Guid id);
    Task<RoleResponse?> GetByIdAsync(Guid id);
    Task AssignToUserAsync(Guid userId, Guid roleId);
    Task RemoveFromUserAsync(Guid userId, Guid roleId);
    Task<List<RoleResponse>> GetUserRolesAsync(Guid userId);
}
