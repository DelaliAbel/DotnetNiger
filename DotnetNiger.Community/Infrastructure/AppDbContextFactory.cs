using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetNiger.Community.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var provider = Environment.GetEnvironmentVariable("DatabaseProvider") ?? "Sqlite";
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? "Data Source=DotnetNigerCommunity.db";

        if (provider == "SqlServer")
            optionsBuilder.UseSqlServer(connStr);
        else
            optionsBuilder.UseSqlite(connStr);

        return new AppDbContext(optionsBuilder.Options);
    }
}
