using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public interface IPermissionService
{
    Task<PermissionResponse> CreateAsync(CreatePermissionRequest request);
    Task<PaginatedResponse<PermissionResponse>> GetByTenantAsync(Guid tenantId, PaginationQuery pagination);
    Task<List<PermissionGroupResponse>> GetGroupedByTenantAsync(Guid tenantId, int page = 1, int pageSize = 200);
    Task<PermissionResponse?> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task AssignToRoleAsync(Guid roleId, List<Guid> permissionIds);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}
