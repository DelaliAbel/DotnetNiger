using DotnetNiger.Api.Data;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Services.General;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;

namespace DotnetNiger.Api.Seed;

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

    public static async Task BootstrapOpenIddictAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var frontendBaseUrl = config.GetValue<string>("FrontendBaseUrl") ?? "http://localhost:5201";
        var mgmt = scope.ServiceProvider.GetRequiredService<OpenIddictManagementService>();
        await mgmt.BootstrapWebUiAsync(appManager, frontendBaseUrl);
    }
}
