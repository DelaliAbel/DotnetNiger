namespace DotnetNiger.Community.Infrastructure.Data.Seeds;

public static class DefaultDataSeeder
{
    public static async Task SeedAsync(CommunityDbContext context, CancellationToken cancellationToken = default)
    {
        await CategorySeeder.SeedAsync(context, cancellationToken);
        await TagSeeder.SeedAsync(context, cancellationToken);
        await PartnerSeeder.SeedAsync(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
