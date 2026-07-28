using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des utilisateurs.</summary>
public interface IUserService
{
    /// <summary>Crée un nouvel utilisateur.</summary>
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    /// <summary>Récupère un utilisateur par identifiant.</summary>
    Task<UserResponse?> GetByIdAsync(Guid id);
    /// <summary>Récupère la liste paginée des utilisateurs.</summary>
    Task<PaginatedResponse<UserResponse>> GetAllAsync(PaginationQuery pagination);
    /// <summary>Met à jour le profil d'un utilisateur.</summary>
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
    /// <summary>Supprime un utilisateur.</summary>
    Task DeleteAsync(Guid id);
    /// <summary>Change le mot de passe d'un utilisateur.</summary>
    Task<UserResponse> ChangePasswordAsync(Guid id, ChangePasswordRequest request);
    /// <summary>Envoie un email de réinitialisation de mot de passe.</summary>
    Task ForgotPasswordAsync(string email);
    /// <summary>Réinitialise le mot de passe avec un token.</summary>
    Task ResetPasswordAsync(string email, string token, string newPassword);
}
