using DotnetNiger.Api.Infrastructure.Data;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Api.Seed;


/// <summary>
/// Seed initial de la base de données.
/// Initialise les rôles, permissions, compte admin et contenu sample.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, string adminPassword)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await db.Database.MigrateAsync();

        await RolesSeeder.SeedAsync(roleManager);
        await PermissionsSeeder.SeedAsync(db, roleManager);
        await AdminUser.SeedAsync(userManager, adminPassword);
    }
}
