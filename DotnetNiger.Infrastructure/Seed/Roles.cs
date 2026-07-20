using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Infrastructure.Seed;

public static class RolesSeeder
{
    private static readonly string[] SeedRoles = ["SuperAdmin", "Admin", "User", "Collaborator"];

    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in SeedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }
}
