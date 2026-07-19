using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DotnetNigerDbContext>
{
    public DotnetNigerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DotnetNigerDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost; Database=DotnetNiger; User Id=SA; Password=SqlServer2026!; TrustServerCertificate=True;");
        optionsBuilder.UseOpenIddict();
        return new DotnetNigerDbContext(optionsBuilder.Options);
    }
}
