using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Community;

public interface IPartnerService
{
    Task<List<PartnerResponse>> GetAllActiveAsync(string? partnerType);
    Task<PartnerResponse?> GetByIdAsync(Guid id);
    Task<PartnerResponse> CreateAsync(CreatePartnerRequest request);
    Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request);
    Task<bool> DeleteAsync(Guid id);
}
