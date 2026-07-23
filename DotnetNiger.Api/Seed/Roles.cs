using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Seed;

/// <summary>
/// Seeder des rôles utilisateur de l'application.
/// </summary>
public static class RolesSeeder
{
    private static readonly string[] SeedRoles = ["SuperAdmin", "Admin", "User", "Collaborator"];

    /// <summary>
    /// Crée les rôles par défaut s'ils n'existent pas.
    /// </summary>
    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in SeedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }
}
