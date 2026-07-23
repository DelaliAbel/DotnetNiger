using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Seed;

/// <summary>
/// Seed de l'utilisateur administrateur initial (SuperAdmin).
/// </summary>
public static class AdminUser
{
    /// <summary>Email de l'administrateur.</summary>
    public const string Email = "admin@dotnetniger.org";
    /// <summary>Mot de passe de l'administrateur.</summary>
    public const string Password = "Admin@123456";
    /// <summary>Rôle de l'administrateur.</summary>
    public const string Role = "SuperAdmin";

    /// <summary>Identifiant de l'administrateur après création.</summary>
    public static Guid? AdminId { get; private set; }

    /// <summary>
    /// Crée l'utilisateur admin s'il n'existe pas déjà.
    /// </summary>
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync(Email) != null)
            return;

        var admin = new ApplicationUser
        {
            UserName = Email,
            Email = Email,
            FirstName = "Admin",
            LastName = "DotnetNiger",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Role);
            AdminId = admin.Id;
        }
    }
}
