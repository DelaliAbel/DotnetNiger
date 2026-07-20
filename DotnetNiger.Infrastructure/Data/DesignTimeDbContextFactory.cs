using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DotnetNigerDbContext>
{
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
        optionsBuilder.UseOpenIddict();
        return new DotnetNigerDbContext(optionsBuilder.Options);
    }
}
