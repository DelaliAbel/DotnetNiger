using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Admin;

public interface IPermissionService
{
    Task<PermissionResponse> CreateAsync(CreatePermissionRequest request);
    Task<PaginatedResponse<PermissionResponse>> GetAllAsync(PaginationQuery pagination);
    Task<List<PermissionGroupResponse>> GetGroupedAsync(int page = 1, int pageSize = 200);
    Task<PermissionResponse?> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task AssignToRoleAsync(Guid roleId, List<Guid> permissionIds);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}
