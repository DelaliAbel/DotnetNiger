using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des partenaires de la communauté.</summary>
public interface IPartnerService
{
    /// <summary>Liste des partenaires actifs, éventuellement filtrés par type.</summary>
    Task<List<PartnerResponse>> GetAllActiveAsync(string? partnerType);
    /// <summary>Détail d'un partenaire.</summary>
    Task<PartnerResponse?> GetByIdAsync(Guid id);
    /// <summary>Ajoute un nouveau partenaire.</summary>
    Task<PartnerResponse> CreateAsync(CreatePartnerRequest request);
    /// <summary>Modifie un partenaire existant.</summary>
    Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request);
    /// <summary>Supprime un partenaire.</summary>
    Task<bool> DeleteAsync(Guid id);
}
