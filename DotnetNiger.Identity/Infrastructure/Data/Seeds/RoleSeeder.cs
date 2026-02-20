// Seed Identity: RoleSeeder
using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Identity.Infrastructure.Data.Seeds;

/// <summary>
/// Cree les roles par defaut (Admin, Member) si absents.
/// </summary>
public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        string[] roles = ["Admin", "Member"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new Role(roleName));
                if (result.Succeeded)
                {
                    logger.LogInformation("Role '{RoleName}' cree avec succes.", roleName);
                }
                else
                {
                    logger.LogWarning("Echec creation du role '{RoleName}': {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
