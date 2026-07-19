using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Seed;

public static class MigrationRunner
{
    public static async Task RunAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<DotnetNigerDbContext>();
        Console.WriteLine(">> Running database migrations...");
        await db.Database.MigrateAsync();
        Console.WriteLine("   Migrations applied successfully.");
    }
}