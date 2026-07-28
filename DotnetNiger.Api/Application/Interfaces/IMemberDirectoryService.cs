using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service d'annuaire des membres.</summary>
public interface IMemberDirectoryService
{
    /// <summary>Récupère le profil membre d'un utilisateur.</summary>
    Task<MemberResponse> GetProfileAsync(Guid userId);
    /// <summary>Met à jour le profil membre.</summary>
    Task<MemberResponse> UpdateProfileAsync(Guid userId, UpdateMemberRequest request);
    /// <summary>Crée un profil membre.</summary>
    Task<MemberResponse> CreateProfileAsync(Guid userId, CreateMemberRequest request);
    /// <summary>Supprime le profil membre.</summary>
    Task<bool> DeleteProfileAsync(Guid userId);
    /// <summary>Récupère les membres paginés.</summary>
    Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? query, string? country, int page, int pageSize);
    /// <summary>Récupère les membres de l'équipe.</summary>
    Task<List<MemberResponse>> GetTeamMembersAsync();
    /// <summary>Récupère un membre par identifiant.</summary>
    Task<MemberResponse?> GetByIdAsync(Guid id);
    /// <summary>Recherche des membres.</summary>
    Task<PaginatedResponse<MemberResponse>> SearchAsync(string? query, int page, int pageSize);
    /// <summary>Ajoute une compétence à un membre.</summary>
    Task AddSkillAsync(Guid userId, string skillName);
    /// <summary>Retire une compétence d'un membre.</summary>
    Task RemoveSkillAsync(Guid userId, string skillName);
}
