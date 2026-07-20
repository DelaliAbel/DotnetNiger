using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Infrastructure.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Set<Category>().AnyAsync())
            return;

        await RolesSeeder.SeedAsync(roleManager);

        await AdminUser.SeedAsync(userManager);
        if (AdminUser.AdminId == null)
            return;

        await SampleContent.SeedAsync(db, AdminUser.AdminId.Value);
    }
}
