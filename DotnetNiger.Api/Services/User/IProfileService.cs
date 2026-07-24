using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.User;

/// <summary>Interface du service de gestion du profil utilisateur.</summary>
public interface IProfileService
{
    /// <summary>Récupère le profil d'un utilisateur.</summary>
    Task<ProfileResponse?> GetAsync(Guid userId);
    /// <summary>Met à jour le profil utilisateur.</summary>
    Task<ProfileResponse?> UpdateAsync(Guid userId, UpdateProfileRequest request);
    /// <summary>Récupère les liens sociaux du profil.</summary>
    Task<List<SocialLinkResponse>> GetSocialLinksAsync(Guid userId);
    /// <summary>Ajoute un lien social au profil.</summary>
    Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request);
    /// <summary>Supprime un lien social du profil.</summary>
    Task<bool> DeleteSocialLinkAsync(Guid userId, Guid linkId);
}
