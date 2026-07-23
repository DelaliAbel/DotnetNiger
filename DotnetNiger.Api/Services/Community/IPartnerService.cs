using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Community;

/// <summary>Interface du service de gestion des partenaires.</summary>
public interface IPartnerService
{
    /// <summary>Récupère les partenaires actifs.</summary>
    Task<List<PartnerResponse>> GetAllActiveAsync(string? partnerType);
    /// <summary>Récupère un partenaire par identifiant.</summary>
    Task<PartnerResponse?> GetByIdAsync(Guid id);
    /// <summary>Crée un partenaire.</summary>
    Task<PartnerResponse> CreateAsync(CreatePartnerRequest request);
    /// <summary>Met à jour un partenaire.</summary>
    Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request);
    /// <summary>Supprime un partenaire.</summary>
    Task<bool> DeleteAsync(Guid id);
}
