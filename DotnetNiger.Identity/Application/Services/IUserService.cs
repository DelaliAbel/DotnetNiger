using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse?> GetByIdAsync(Guid tenantId, Guid id);
    Task<PaginatedResponse<UserResponse>> GetByTenantAsync(Guid tenantId, PaginationQuery pagination);
    Task<UserResponse> UpdateAsync(Guid tenantId, Guid id, UpdateUserRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
    Task<UserResponse> ChangePasswordAsync(Guid tenantId, Guid id, ChangePasswordRequest request);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string email, string token, string newPassword);
}
