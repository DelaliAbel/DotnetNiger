// Acces donnees Identity: DotnetNigerIdentityDbFactory
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetNiger.Identity.Infrastructure.Data;

/// <summary>
/// Factory design-time pour les commandes EF Core CLI (migrations, database update, etc.).
/// Utilisee automatiquement par 'dotnet ef' quand aucun host n'est disponible.
/// </summary>
public class DotnetNigerIdentityDbFactory : IDesignTimeDbContextFactory<DotnetNigerIdentityDbContext>
{
    public DotnetNigerIdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DotnetNigerIdentityDbContext")
            ?? throw new InvalidOperationException(
                "Connection string 'DotnetNigerIdentityDbContext' not found in appsettings.");

        var optionsBuilder = new DbContextOptionsBuilder<DotnetNigerIdentityDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new DotnetNigerIdentityDbContext(optionsBuilder.Options);
    }
}
