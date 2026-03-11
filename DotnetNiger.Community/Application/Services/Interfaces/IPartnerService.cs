using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IPartnerService
{
    Task<IEnumerable<Partner>> GetAllPartnersAsync();
    Task<Partner?> GetPartnerByIdAsync(Guid id);
    Task<Partner> CreatePartnerAsync(Partner partner);
    Task<Partner> UpdatePartnerAsync(Partner partner);
    Task<bool> DeletePartnerAsync(Guid id);
}
