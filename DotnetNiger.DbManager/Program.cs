using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.DbManager;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connStr = context.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required");
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseSqlServer(connStr, x => x.MigrationsAssembly("DotnetNiger.Identity"));
            options.UseOpenIddict();
        });
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connStr, x =>
            {
                x.MigrationsAssembly("DotnetNiger.Community");
                x.MigrationsHistoryTable("__EFMigrationsHistory_Community");
            }));
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();
        services.AddOpenIddict()
            .AddCore(core => core.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>());
        services.AddScoped<TenantContext>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    await MigrationRunner.RunAsync(scope.ServiceProvider);
    await SeedIdentityService.SeedAsync(scope.ServiceProvider);
    await ClientSetupService.SetupAsync(scope.ServiceProvider);
    await SeedCommunityService.SeedAsync(scope.ServiceProvider);
    Console.WriteLine("\n=== Database setup complete ===");
}
