using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Services.Auth;

public partial class AuthService
{
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
