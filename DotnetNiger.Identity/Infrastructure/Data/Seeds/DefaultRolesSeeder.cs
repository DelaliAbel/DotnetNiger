// Seed Identity: DefaultRolesSeeder
using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Identity.Infrastructure.Data.Seeds;

/// <summary>
/// Orchestre le seeding complet : roles, permissions, assignation des permissions au role Admin,
/// et creation du compte admin par defaut.
/// </summary>
public static class DefaultRolesSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<DotnetNigerIdentityDbContext>();

        // 1. Seed des roles
        await RoleSeeder.SeedAsync(roleManager, logger);

        // 2. Seed des permissions
        await PermissionSeeder.SeedAsync(context, logger);

        // 3. Assigner toutes les permissions au role Admin
        await AssignAllPermissionsToAdminAsync(context, roleManager, logger);

        // 4. Creer le compte admin par defaut
        await SeedAdminUserAsync(userManager, logger);
    }

    private static async Task AssignAllPermissionsToAdminAsync(
        DotnetNigerIdentityDbContext context,
        RoleManager<Role> roleManager,
        ILogger logger)
    {
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole is null)
        {
            logger.LogWarning("Role 'Admin' introuvable — impossible d'assigner les permissions.");
            return;
        }

        var allPermissions = await context.Permissions.ToListAsync();
        var existingLinks = await context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        foreach (var permission in allPermissions)
        {
            if (!existingLinks.Contains(permission.Id))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
                logger.LogInformation("Permission '{Permission}' assignee au role Admin.", permission.Name);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@dotnetniger.com";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin2026@DotnetNiger";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            logger.LogInformation("Compte admin '{Email}' existe deja.", adminEmail);
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FullName = "Administrateur DotnetNiger",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Compte admin '{Email}' cree et assigne au role Admin.", adminEmail);
        }
        else
        {
            logger.LogWarning("Echec creation admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
