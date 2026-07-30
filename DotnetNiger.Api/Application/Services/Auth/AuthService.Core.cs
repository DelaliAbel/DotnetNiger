using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Application.Services.Auth;

/// <summary>Validation des identifiants et gestion de la connexion utilisateur.</summary>
public partial class AuthService
{
    /// <summary>Valide les identifiants de connexion et retourne l'utilisateur avec ses rôles.</summary>
    public async Task<(ApplicationUser user, IList<string> roles)> ValidateCredentialsAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect");
        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new UnauthorizedAccessException("Email non confirmé");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);
        if (result.IsLockedOut)
            throw new UnauthorizedAccessException("Compte temporairement verrouillé");
        if (result.IsNotAllowed)
            throw new UnauthorizedAccessException("Connexion non autorisée - vérifiez que votre email est confirmé");
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect");

        var roles = await _userManager.GetRolesAsync(user);
        return (user, roles);
    }

}
