using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public interface ITenantApiKeyService
{
    Task<TenantApiKeyCreatedResponse> CreateApiKeyAsync(Guid tenantId, CreateTenantApiKeyRequest request);
    Task<PaginatedResponse<TenantApiKeyResponse>> GetApiKeysAsync(Guid tenantId, PaginationQuery pagination);
    Task<TenantApiKeyResponse> GetApiKeyByIdAsync(Guid tenantId, Guid keyId);
    Task DeleteApiKeyAsync(Guid tenantId, Guid keyId);
    Task<TenantApiKeyCreatedResponse> RotateApiKeyAsync(Guid tenantId, Guid keyId);
}
