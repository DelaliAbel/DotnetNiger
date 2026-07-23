using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Seed;

/// <summary>
/// Seeder des permissions et de leur assignation aux rôles.
/// </summary>
public static class PermissionsSeeder
{
    /// <summary>
    /// Insère les permissions et les associe aux rôles correspondants.
    /// </summary>
    public static async Task SeedAsync(DotnetNigerDbContext db, RoleManager<ApplicationRole> roleManager)
    {
        var existingCount = await db.Permissions.CountAsync();
        if (existingCount > 0)
            return;

        var permissionEntities = new List<Permission>();
        foreach (var name in Permissions.All)
        {
            var category = name.Split('.')[0];
            permissionEntities.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = name,
                Category = category
            });
        }

        db.Permissions.AddRange(permissionEntities);
        await db.SaveChangesAsync();

        var superAdminRole = await roleManager.FindByNameAsync(RoleConstants.SuperAdmin);
        var adminRole = await roleManager.FindByNameAsync(RoleConstants.Admin);
        var collaboratorRole = await roleManager.FindByNameAsync(RoleConstants.Collaborator);
        var userRole = await roleManager.FindByNameAsync(RoleConstants.User);

        var rolePermissions = db.Set<Dictionary<string, object>>("RolePermission");

        foreach (var perm in permissionEntities)
        {
            if (superAdminRole != null && Permissions.SuperAdminPermissions.Contains(perm.Name))
            {
                rolePermissions.Add(new Dictionary<string, object>
                {
                    ["RoleId"] = superAdminRole.Id,
                    ["PermissionId"] = perm.Id
                });
            }

            if (adminRole != null && Permissions.AdminPermissions.Contains(perm.Name))
            {
                rolePermissions.Add(new Dictionary<string, object>
                {
                    ["RoleId"] = adminRole.Id,
                    ["PermissionId"] = perm.Id
                });
            }

            if (collaboratorRole != null && Permissions.CollaboratorPermissions.Contains(perm.Name))
            {
                rolePermissions.Add(new Dictionary<string, object>
                {
                    ["RoleId"] = collaboratorRole.Id,
                    ["PermissionId"] = perm.Id
                });
            }

            if (userRole != null && Permissions.UserPermissions.Contains(perm.Name))
            {
                rolePermissions.Add(new Dictionary<string, object>
                {
                    ["RoleId"] = userRole.Id,
                    ["PermissionId"] = perm.Id
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
