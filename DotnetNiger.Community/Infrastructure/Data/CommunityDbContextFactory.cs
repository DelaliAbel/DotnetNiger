using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Community.Infrastructure.Data;

public class CommunityDbContextFactory : IDesignTimeDbContextFactory<CommunityDbContext>
{
    public CommunityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DotnetNigerDb")
            ?? "Data Source=../DotnetNiger.db";

        var optionsBuilder = new DbContextOptionsBuilder<CommunityDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new CommunityDbContext(optionsBuilder.Options);
    }
}
