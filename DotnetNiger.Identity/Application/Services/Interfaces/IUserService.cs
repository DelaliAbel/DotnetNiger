// Contrat applicatif Identity: IUserService
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

// Contrat pour les operations utilisateur.
public interface IUserService
{
    // Profil utilisateur courant.
    Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken ct = default);

    // Mise a jour du profil utilisateur.
    Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    // Mise a jour de l'avatar utilisateur.
    Task<UserResponse> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct = default);

    // Suppression de l'avatar utilisateur.
    Task<UserResponse> ClearAvatarAsync(Guid userId, CancellationToken ct = default);

    // Changement de mot de passe.
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);

    // Changement d'email.
    Task ChangeEmailAsync(Guid userId, ChangeEmailRequest request, CancellationToken ct = default);
}
