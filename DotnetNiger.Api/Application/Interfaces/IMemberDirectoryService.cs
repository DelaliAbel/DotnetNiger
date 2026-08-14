using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service d'annuaire des membres.</summary>
public interface IMemberDirectoryService
{
    /// <summary>Récupère le profil membre d'un utilisateur.</summary>
    Task<MemberResponse> GetProfileAsync(Guid userId, CancellationToken ct = default);
    /// <summary>Met à jour le profil membre.</summary>
    Task<MemberResponse> UpdateProfileAsync(Guid userId, UpdateMemberRequest request, CancellationToken ct = default);
    /// <summary>Crée un profil membre.</summary>
    Task<MemberResponse> CreateProfileAsync(Guid userId, CreateMemberRequest request, CancellationToken ct = default);
    /// <summary>Supprime le profil membre.</summary>
    Task<bool> DeleteProfileAsync(Guid userId, CancellationToken ct = default);
    /// <summary>Récupère les membres paginés.</summary>
    Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? query, string? country, int page, int pageSize, CancellationToken ct = default);
    /// <summary>Récupère les membres de l'équipe.</summary>
    Task<List<MemberResponse>> GetTeamMembersAsync(CancellationToken ct = default);
    /// <summary>Récupère un membre par identifiant.</summary>
    Task<MemberResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Recherche des membres.</summary>
    Task<PaginatedResponse<MemberResponse>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default);
    /// <summary>Ajoute une compétence à un membre.</summary>
    Task AddSkillAsync(Guid userId, string skillName, CancellationToken ct = default);
    /// <summary>Retire une compétence d'un membre.</summary>
    Task RemoveSkillAsync(Guid userId, string skillName, CancellationToken ct = default);
}
