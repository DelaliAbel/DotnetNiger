using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Data.Seeds;

public static class PartnerSeeder
{
    public static async Task SeedAsync(CommunityDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Partners.AnyAsync(cancellationToken))
            return;

        var partners = new[]
        {
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Dotnet Foundation",
                Slug = "dotnet-foundation",
                LogoUrl = "/images/partners/dotnet-foundation.png",
                Website = "https://dotnetfoundation.org",
                Description = "Partenaire communautaire",
                PartnerType = "Partner",
                Level = "Gold",
                DisplayOrder = 1,
                IsActive = true
            }
        };

        await context.Partners.AddRangeAsync(partners, cancellationToken);
    }
}
