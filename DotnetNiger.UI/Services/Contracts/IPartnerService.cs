using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IPartnerService
{
    Task<List<PartnerResponse>> GetAllActiveAsync(string? partnerType);
    Task<List<PartnerResponse>> GetAllAsync();
    Task<PartnerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PartnerResponse?> CreateAsync(CreatePartnerRequest request, CancellationToken cancellationToken = default);
    Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
