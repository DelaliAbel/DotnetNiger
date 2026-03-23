using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Data.Seeds;

public static class CategorySeeder
{
    public static async Task SeedAsync(CommunityDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
            return;

        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Dotnet", Slug = "dotnet", Description = "Contenu Dotnet" },
            new Category { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp", Description = "Contenu CSharp" },
            new Category { Id = Guid.NewGuid(), Name = "Cloud", Slug = "cloud", Description = "Contenu Cloud" }
        };

        await context.Categories.AddRangeAsync(categories, cancellationToken);
    }
}
