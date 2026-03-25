// Seed Identity: PermissionSeeder
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Identity.Infrastructure.Data.Seeds;

/// <summary>
/// Cree les permissions par defaut si absentes.
/// </summary>
public static class PermissionSeeder
{
    private static readonly (string Name, string Description)[] DefaultPermissions =
    [
        ("users.read", "Consulter la liste des utilisateurs"),
        ("users.write", "Modifier les utilisateurs"),
        ("users.delete", "Supprimer des utilisateurs"),
        ("roles.manage", "Gerer les roles et assignations"),
        ("permissions.manage", "Gerer les permissions"),
        ("apikeys.manage", "Gerer les cles API"),
        ("audit.read", "Consulter les logs d'audit"),
    ];

    public static async Task SeedAsync(DotnetNigerIdentityDbContext context, ILogger logger)
    {
        foreach (var (name, description) in DefaultPermissions)
        {
            if (!await context.Permissions.AnyAsync(p => p.Name == name))
            {
                context.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });
                logger.LogInformation("Permission '{PermissionName}' creee.", name);
            }
        }

        await context.SaveChangesAsync();
    }
}
