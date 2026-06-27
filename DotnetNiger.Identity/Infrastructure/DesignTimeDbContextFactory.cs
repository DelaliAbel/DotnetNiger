using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("DefaultConnection")
            ?? "Server=localhost; Database=DotnetNiger; User Id=SA; Password=SqlServer2026!; TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<IdentityDbContext>();
        options.UseSqlServer(connStr, x => x.MigrationsAssembly("DotnetNiger.Identity"));

        options.UseOpenIddict();

        return new IdentityDbContext(options.Options, new TenantContext());
    }
}
