using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Seed;

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
