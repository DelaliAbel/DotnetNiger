using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetNiger.Community.Infrastructure.Data;

public class CommunityDbContextFactory : IDesignTimeDbContextFactory<CommunityDbContext>
{
    public CommunityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommunityDbContext>();

        // Utiliser une base de données SQLite locale pour les migrations
        var connectionString = "Data Source=community.db";
        optionsBuilder.UseSqlite(connectionString);

        return new CommunityDbContext(optionsBuilder.Options);
    }
}
