using System.Security.Claims;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface IExternalServiceService
{
    Task<ExternalServiceResponse> RegisterAsync(RegisterExternalServiceRequest request);
    Task<PaginatedResponse<ExternalServiceResponse>> GetAllAsync(PaginationQuery pagination);
    Task<ExternalServiceResponse> GetByIdAsync(Guid serviceId);
    Task<ServiceLookupResult?> ResolveSlugAsync(string slug);
    Task<ExternalServiceResponse> UpdateAsync(Guid serviceId, UpdateExternalServiceRequest request);
    Task<List<ExternalServiceResponse>> GetAllActiveAsync(int page = 1, int pageSize = 50);
    Task UpdateHealthStatusAsync(Guid serviceId, bool isHealthy);
    Task DeleteAsync(Guid serviceId);
    Task<Guid?> GetAuthInfoAsync(ClaimsPrincipal user);
}
