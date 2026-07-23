using DotnetNiger.Api.Data;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Services.General;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Api.Seed;

/// <summary>
/// Seed initial de la base de données.
/// Initialise les rôles, permissions, compte admin et contenu sample.
/// Plus de bootstrap OpenIddict — la configuration est maintenant dans appsettings.json.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await db.Database.MigrateAsync();

        if (await db.Set<Category>().AnyAsync())
            return;

        await RolesSeeder.SeedAsync(roleManager);
        await PermissionsSeeder.SeedAsync(db, roleManager);
        await AdminUser.SeedAsync(userManager);

        if (AdminUser.AdminId == null)
            return;

        await SampleContent.SeedAsync(db, AdminUser.AdminId.Value);
    }
}
