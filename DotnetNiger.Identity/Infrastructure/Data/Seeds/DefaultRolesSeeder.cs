// Seed Identity: DefaultRolesSeeder
using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Identity.Infrastructure.Data.Seeds;

/// <summary>
/// Orchestre le seeding des roles/permissions et garantit un socle coherent au demarrage.
/// </summary>
public static class DefaultRolesSeeder
{
    public static async Task EnsureCoreDataAsync(
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

        // 3. Assigner toutes les permissions uniquement au role SuperAdmin par defaut
        await AssignAllPermissionsToSuperAdminAsync(context, roleManager, logger);
    }

    public static async Task<bool> SuperAdminExistsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var users = await userManager.GetUsersInRoleAsync("SuperAdmin");
        return users.Count > 0;
    }

    private static async Task AssignAllPermissionsToSuperAdminAsync(
        DotnetNigerIdentityDbContext context,
        RoleManager<Role> roleManager,
        ILogger logger)
    {
        var privilegedRoleNames = new[] { "SuperAdmin" };
        var allPermissions = await context.Permissions.ToListAsync();

        foreach (var roleName in privilegedRoleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                logger.LogWarning("Role '{RoleName}' introuvable — impossible d'assigner les permissions.", roleName);
                continue;
            }

            var existingLinks = await context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            foreach (var permission in allPermissions)
            {
                if (!existingLinks.Contains(permission.Id))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                    logger.LogInformation("Permission '{Permission}' assignee au role {RoleName}.", permission.Name, roleName);
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
