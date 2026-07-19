using System.Security.Claims;
using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public interface IExternalServiceService
{
    Task<ExternalServiceResponse> RegisterAsync(Guid tenantId, Guid apiKeyId, RegisterExternalServiceRequest request);
    Task<PaginatedResponse<ExternalServiceResponse>> GetByTenantAsync(Guid tenantId, PaginationQuery pagination);
    Task<ExternalServiceResponse> GetByIdAsync(Guid tenantId, Guid serviceId);
    Task<ServiceLookupResult?> ResolveSlugAsync(string slug);
    Task<ExternalServiceResponse> UpdateAsync(Guid tenantId, Guid serviceId, UpdateExternalServiceRequest request);
    Task<List<ExternalServiceResponse>> GetAllActiveAsync(int page = 1, int pageSize = 50);
    Task UpdateHealthStatusAsync(Guid serviceId, bool isHealthy);
    Task DeleteAsync(Guid tenantId, Guid serviceId);
    Task<(Guid tenantId, Guid? apiKeyId)> GetAuthInfoAsync(ClaimsPrincipal user);
}
