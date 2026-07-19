using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface IApiKeyService
{
    Task<ApiKeyCreatedResponse> CreateApiKeyAsync(CreateApiKeyRequest request);
    Task<PaginatedResponse<ApiKeyResponse>> GetApiKeysAsync(PaginationQuery pagination);
    Task<ApiKeyResponse> GetApiKeyByIdAsync(Guid keyId);
    Task DeleteApiKeyAsync(Guid keyId);
    Task<ApiKeyCreatedResponse> RotateApiKeyAsync(Guid keyId);
}
