using DotnetNiger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetNiger.Server.Data;

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
