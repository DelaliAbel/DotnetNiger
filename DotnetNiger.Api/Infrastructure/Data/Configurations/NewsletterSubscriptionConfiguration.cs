using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité NewsletterSubscription.
/// </summary>
public class NewsletterSubscriptionConfiguration : IEntityTypeConfiguration<NewsletterSubscription>
{
    /// <summary>
    /// Configure les clés et indexes de la table des abonnements newsletter.
    /// </summary>
    public void Configure(EntityTypeBuilder<NewsletterSubscription> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(n => n.Email).IsUnique();
    }
}
