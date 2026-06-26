using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des profils membres et des certificats.</summary>
public interface IProfileService
{
    /// <summary>Profil complet d'un membre avec ses compétences et liens sociaux.</summary>
    Task<ProfileResponse?> GetAsync(Guid userId);
    /// <summary>Met à jour le profil (le crée s'il n'existe pas encore).</summary>
    Task<ProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request);
    /// <summary>Ajoute un lien social au profil du membre.</summary>
    Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request);
    /// <summary>Supprime un lien social du profil.</summary>
    Task<bool> DeleteSocialLinkAsync(Guid userId, Guid socialLinkId);
    /// <summary>Soumet un certificat pour validation par un admin.</summary>
    Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request);
    /// <summary>Approuve un certificat soumis.</summary>
    Task<CertificateResponse?> ApproveCertificateAsync(Guid certificateId);
    /// <summary>Rejette un certificat avec un motif.</summary>
    Task<CertificateResponse?> RejectCertificateAsync(Guid certificateId, string reason);
    /// <summary>Vérifie si l'utilisateur possède un certificat approuvé.</summary>
    Task<bool> HasApprovedCertificateAsync(Guid userId);
    /// <summary>Liste des certificats avec filtre optionnel par statut.</summary>
    Task<List<CertificateAdminDto>> GetCertificatesAsync(string? status = null);
}
