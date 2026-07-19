using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Seed;

public static class SeedIdentityService
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<DotnetNigerDbContext>();
        var usrMgr = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        var existingAdmin = await usrMgr.FindByEmailAsync("admin@dotnetniger.com");
        if (existingAdmin != null)
        {
            if (string.IsNullOrEmpty(existingAdmin.SecurityStamp))
            {
                Console.WriteLine("   Identity: fixing admin SecurityStamp...");
                existingAdmin.SecurityStamp = Guid.NewGuid().ToString();
                await usrMgr.UpdateAsync(existingAdmin);
            }
        }

        if (await db.Roles.AnyAsync())
        {
            Console.WriteLine("   Identity: already seeded, skipping.");
            return;
        }
        Console.WriteLine(">> Identity: seeding data...");
        var userManager = usrMgr;
        var config = services.GetRequiredService<IConfiguration>();
        var adminPassword = config["Admin:DefaultPassword"] ?? "Admin@123456";

        var roles = new[]
        {
            new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Description = "Super administrateur de la plateforme" },
            new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN", Description = "Administrateur de la plateforme" },
            new ApplicationRole { Name = "User", NormalizedName = "USER", Description = "Utilisateur standard" },
            new ApplicationRole { Name = "Collaborator", NormalizedName = "COLLABORATOR", Description = "Contributeur certifié" },
        };
        foreach (var role in roles)
            await roleManager.CreateAsync(role);

        var permissions = PermissionNames.All.Select(name => new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = PermissionNames.CategoryOf(name),
        }).ToList();
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        var rolePermission = db.Set<Dictionary<string, object>>("RolePermission");
        var saRoleId = roles[0].Id;
        var adminRoleId = roles[1].Id;
        var collabRoleId = roles[3].Id;
        foreach (var perm in permissions)
            rolePermission.Add(new() { ["RoleId"] = saRoleId, ["PermissionId"] = perm.Id });
        foreach (var perm in permissions.Where(p => PermissionNames.AdminPermissions.Contains(p.Name)))
            rolePermission.Add(new() { ["RoleId"] = adminRoleId, ["PermissionId"] = perm.Id });
        foreach (var perm in permissions.Where(p => PermissionNames.CollaboratorPermissions.Contains(p.Name)))
            rolePermission.Add(new() { ["RoleId"] = collabRoleId, ["PermissionId"] = perm.Id });
        await db.SaveChangesAsync();

        var adminResult = await userManager.CreateAsync(
            new ApplicationUser
            {
                Id = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"),
                UserName = "admin@dotnetniger.com",
                Email = "admin@dotnetniger.com",
                FirstName = "Admin",
                LastName = "Plateforme",
                IsActive = true,
                EmailConfirmed = true,
            }, adminPassword);
        if (!adminResult.Succeeded)
            throw new InvalidOperationException($"Admin create failed: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}");

        var adminUser = await userManager.FindByEmailAsync("admin@dotnetniger.com")!;
        await userManager.AddToRoleAsync(adminUser!, "SuperAdmin");

        var testResult = await userManager.CreateAsync(
            new ApplicationUser
            {
                Id = Guid.Parse("B2C3D4E5-F6A7-8901-BCDE-F12345678901"),
                UserName = "test@dotnetniger.com",
                Email = "test@dotnetniger.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                EmailConfirmed = true,
            }, "Test@123456");
        if (!testResult.Succeeded)
            throw new InvalidOperationException($"Test user create failed: {string.Join(", ", testResult.Errors.Select(e => e.Description))}");

        var testUser = await userManager.FindByEmailAsync("test@dotnetniger.com")!;
        await userManager.AddToRoleAsync(testUser!, "User");
        Console.WriteLine("   Identity: seed complete.");
    }
}

public static class PermissionNames
{
    public static readonly string[] All =
    {
        "users.read", "users.write", "users.delete",
        "roles.read", "roles.write", "roles.delete",
        "permissions.read", "permissions.write", "permissions.delete",
        "oauthclients.read", "oauthclients.write", "oauthclients.delete",
        "apikeys.read", "apikeys.write", "apikeys.delete",
        "externalservices.read", "externalservices.write", "externalservices.delete",
        "auditlogs.read", "auditlogs.write", "auditlogs.delete",
        "posts.read", "posts.write", "posts.delete", "posts.publish", "posts.moderate",
        "categories.read", "categories.write", "categories.delete",
        "tags.read", "tags.write", "tags.delete",
        "events.read", "events.write", "events.delete", "events.manage",
        "resources.read", "resources.write", "resources.delete",
        "members.read", "members.write", "members.delete",
        "projects.read", "projects.write", "projects.delete",
        "partners.read", "partners.write", "partners.delete",
        "newsletters.read", "newsletters.write", "newsletters.delete",
        "certificates.read", "certificates.write", "certificates.delete",
        "settings.read", "settings.write", "settings.delete",
        "contact.read", "contact.write", "contact.delete",
        "support.read", "support.write", "support.delete",
        "notifications.read", "notifications.write", "notifications.delete",
        "dashboard.read", "dashboard.write",
        "reports.read", "reports.write",
    };

    public static readonly string[] AdminPermissions = All.Where(p => p.StartsWith("users.") || p.StartsWith("roles.") || p.StartsWith("permissions.") || p.StartsWith("oauthclients.") || p.StartsWith("apikeys.") || p.StartsWith("externalservices.") || p.StartsWith("auditlogs.") || p.StartsWith("settings.") || p.StartsWith("dashboard.") || p.StartsWith("reports.")).ToArray();

    public static readonly string[] CollaboratorPermissions = All.Where(p => p.StartsWith("posts.write") || p.StartsWith("posts.publish") || p.StartsWith("categories.write") || p.StartsWith("tags.write") || p.StartsWith("events.write") || p.StartsWith("resources.write") || p.StartsWith("members.write") || p.StartsWith("projects.write") || p.StartsWith("partners.write") || p.StartsWith("certificates.write")).ToArray();

    public static string CategoryOf(string name)
    {
        if (name.StartsWith("users.") || name.StartsWith("roles.") || name.StartsWith("permissions.")) return "Identity";
        if (name.StartsWith("oauthclients.") || name.StartsWith("apikeys.") || name.StartsWith("externalservices.") || name.StartsWith("auditlogs.")) return "Security";
        if (name.StartsWith("posts.") || name.StartsWith("categories.") || name.StartsWith("tags.")) return "Content";
        if (name.StartsWith("events.") || name.StartsWith("resources.") || name.StartsWith("members.")) return "Community";
        if (name.StartsWith("projects.") || name.StartsWith("partners.") || name.StartsWith("certificates.")) return "Projects";
        if (name.StartsWith("settings.") || name.StartsWith("dashboard.") || name.StartsWith("reports.")) return "Administration";
        if (name.StartsWith("newsletters.") || name.StartsWith("contact.") || name.StartsWith("support.") || name.StartsWith("notifications.")) return "Communication";
        return "Other";
    }
}