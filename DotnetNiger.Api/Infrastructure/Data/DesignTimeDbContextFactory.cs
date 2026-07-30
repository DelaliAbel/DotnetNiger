using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Api.Infrastructure.Data;

/// <summary>
/// Fabrique de DbContext pour les migrations Entity Framework en mode design-time.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DotnetNigerDbContext>
{
    /// <summary>
    /// Crée une instance de DotnetNigerDbContext pour les migrations EF Core.
    /// </summary>
    public DotnetNigerDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Server=localhost; Database=DotnetNiger; User Id=SA; Password=SqlServer2026!; TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<DotnetNigerDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new DotnetNigerDbContext(optionsBuilder.Options);
    }
}
