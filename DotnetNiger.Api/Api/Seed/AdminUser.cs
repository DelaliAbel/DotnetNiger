using DotnetNiger.Api.Domain.Entities;
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
        var admin = await userManager.FindByEmailAsync(Email);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "DotnetNiger SAdmin",
                Email = Email,
                FirstName = "Admin",
                LastName = "DotnetNiger",
                EmailConfirmed = true,
                IsActive = true
            };


            var result = await userManager.CreateAsync(admin, Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    Console.WriteLine(error.Description);

                return;
            }
        }

        //Ajouter le role meme si l'admin existe deja
        if (!await userManager.IsInRoleAsync(admin, Role))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, Role);
            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }

        AdminId = admin.Id;
    }
}