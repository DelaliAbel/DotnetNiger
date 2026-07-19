using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface IPartnerService
{
    Task<List<PartnerResponse>> GetAllActiveAsync(string? partnerType);
    Task<List<PartnerResponse>> GetAllAsync();
    Task<PartnerResponse?> GetByIdAsync(Guid id);
    Task<PartnerResponse?> CreateAsync(CreatePartnerRequest request);
    Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request);
    Task<bool> DeleteAsync(Guid id);
}
