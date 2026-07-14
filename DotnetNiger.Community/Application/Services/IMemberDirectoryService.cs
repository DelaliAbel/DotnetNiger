using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Annuaire public des membres de la communauté.</summary>
public interface IMemberDirectoryService
{
    /// <summary>Recherche paginée dans l'annuaire (filtres par nom/bio/ville et pays).</summary>
    Task<PaginatedResponse<MemberDirectoryResponse>> GetAllAsync(string? query, string? country, int page = 1, int pageSize = 10);
    /// <summary>Détail d'un membre avec ses liens sociaux.</summary>
    Task<MemberDirectoryResponse?> GetByIdAsync(Guid id);
    /// <summary>Retourne les membres marqués comme faisant partie de l'équipe DotnetNiger.</summary>
    Task<List<TeamMemberResponse>> GetTeamMembersAsync();
}
