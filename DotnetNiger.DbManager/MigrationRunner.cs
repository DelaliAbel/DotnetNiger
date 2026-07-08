using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotnetNiger.Identity.Infrastructure;
using DotnetNiger.Community.Infrastructure;

namespace DotnetNiger.DbManager;

/// <summary>Applique les migrations Entity Framework pour Identity et Community.</summary>
static class MigrationRunner
{
    /// <summary>Exécute les migrations Identity, puis Community.</summary>
    public static async Task RunAsync(IServiceProvider services)
    {
        Console.WriteLine(">> Identity: applying migrations...");
        var identityDb = services.GetRequiredService<IdentityDbContext>();
        await identityDb.Database.MigrateAsync();
        Console.WriteLine("   Identity: migrations applied.");

        Console.WriteLine(">> Community: applying migrations...");
        var communityDb = services.GetRequiredService<AppDbContext>();
        await communityDb.Database.MigrateAsync();
        Console.WriteLine("   Community: migrations applied.");
    }
}
