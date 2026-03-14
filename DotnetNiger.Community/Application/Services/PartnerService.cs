using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class PartnerService : IPartnerService
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly ISlugGenerator _slugGenerator;

    public PartnerService(IPartnerRepository partnerRepository, ISlugGenerator slugGenerator)
    {
        _partnerRepository = partnerRepository;
        _slugGenerator = slugGenerator;
    }

    public async Task<IEnumerable<Partner>> GetAllPartnersAsync()
    {
        return await _partnerRepository.GetAllAsync();
    }

    public async Task<Partner?> GetPartnerByIdAsync(Guid id)
    {
        return await _partnerRepository.GetByIdAsync(id);
    }

    public async Task<Partner> CreatePartnerAsync(Partner partner)
    {
        partner.Id = Guid.NewGuid();
        partner.CreatedAt = DateTime.UtcNow;
        partner.Slug = _slugGenerator.Generate(partner.Name);
        return await _partnerRepository.AddAsync(partner);
    }

    public async Task<Partner> UpdatePartnerAsync(Partner partner)
    {
        partner.Slug = _slugGenerator.Generate(partner.Name);
        return await _partnerRepository.UpdateAsync(partner);
    }

    public async Task<bool> DeletePartnerAsync(Guid id)
    {
        return await _partnerRepository.DeleteAsync(id);
    }
}
