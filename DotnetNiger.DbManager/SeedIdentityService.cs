using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using static DotnetNiger.Identity.Application.PermissionNames;

namespace DotnetNiger.DbManager;

/// <summary>Crée le tenant principal, les rôles, les permissions et l'utilisateur admin.</summary>
static class SeedIdentityService
{
    /// <summary>Seed Identity si aucun tenant n'existe.</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<IdentityDbContext>();
        if (await db.Tenants.AnyAsync())
        {
            Console.WriteLine("   Identity: already seeded, skipping.");
            return;
        }

        Console.WriteLine(">> Identity: seeding data...");
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var tenantContext = services.GetRequiredService<TenantContext>();
        var config = services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var adminPassword = config["Admin:DefaultPassword"] ?? "Admin@123456";

        var platformTenant = new Tenant
        {
            Id = Guid.NewGuid(), Name = "DotnetNiger Community", Slug = "platform",
            Description = "Tenant principal de la Communaute DotnetNiger", IsActive = true
        };
        db.Tenants.Add(platformTenant);
        await db.SaveChangesAsync();
        tenantContext.TenantId = platformTenant.Id;

        var roles = new[]
        {
            new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN", TenantId = platformTenant.Id, Description = "Super administrateur de la plateforme" },
            new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN", TenantId = platformTenant.Id, Description = "Administrateur de la plateforme" },
            new ApplicationRole { Name = "User", NormalizedName = "USER", TenantId = platformTenant.Id, Description = "Utilisateur standard" },
            new ApplicationRole { Name = "Collaborator", NormalizedName = "COLLABORATOR", TenantId = platformTenant.Id, Description = "Contributeur certifié" },
        };
        foreach (var role in roles) await roleManager.CreateAsync(role);

        var permissions = All.Select(name => new Permission
        {
            Id = Guid.NewGuid(), TenantId = platformTenant.Id, Name = name, Category = CategoryOf(name),
        }).ToList();
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        var rp = db.Set<Dictionary<string, object>>("RolePermission");
        var saRoleId = roles[0].Id;
        var adminRoleId = roles[1].Id;
        var collabRoleId = roles[3].Id;
        foreach (var perm in permissions) rp.Add(new() { ["RoleId"] = saRoleId, ["PermissionId"] = perm.Id });
        foreach (var perm in permissions.Where(p => AdminPermissions.Contains(p.Name)))
            rp.Add(new() { ["RoleId"] = adminRoleId, ["PermissionId"] = perm.Id });
        foreach (var perm in permissions.Where(p => CollaboratorPermissions.Contains(p.Name)))
            rp.Add(new() { ["RoleId"] = collabRoleId, ["PermissionId"] = perm.Id });
        await db.SaveChangesAsync();

        var adminUser = new ApplicationUser
        {
            UserName = "admin@dotnetniger.com", Email = "admin@dotnetniger.com",
            FirstName = "Admin", LastName = "Plateforme",
            TenantId = platformTenant.Id, IsActive = true, EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        Console.WriteLine("   Identity: seed complete.");
    }
}
