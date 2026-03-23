using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Data.Seeds;

public static class TagSeeder
{
    public static async Task SeedAsync(CommunityDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Tags.AnyAsync(cancellationToken))
            return;

        var tags = new[]
        {
            new Tag { Id = Guid.NewGuid(), Name = "dotnet8", Slug = "dotnet8", PostCount = 0 },
            new Tag { Id = Guid.NewGuid(), Name = "aspnetcore", Slug = "aspnetcore", PostCount = 0 },
            new Tag { Id = Guid.NewGuid(), Name = "microservices", Slug = "microservices", PostCount = 0 }
        };

        await context.Tags.AddRangeAsync(tags, cancellationToken);
    }
}
