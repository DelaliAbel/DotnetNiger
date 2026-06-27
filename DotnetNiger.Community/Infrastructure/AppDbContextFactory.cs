using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Community.Infrastructure;

/// <summary>Fabrique de contexte EF Core pour les migrations et les outils en ligne de commande.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>Crée une instance de AppDbContext en fonction des variables d'environnement.</summary>
    /// <param name="args">Arguments de la ligne de commande.</param>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("DefaultConnection")
            ?? "Server=localhost; Database=DotnetNiger; User Id=SA; Password=SqlServer2026!; TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connStr);
        return new AppDbContext(optionsBuilder.Options);
    }
}
