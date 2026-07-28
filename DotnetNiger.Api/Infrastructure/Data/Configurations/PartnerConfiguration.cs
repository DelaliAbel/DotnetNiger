using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Partner.
/// </summary>
public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    /// <summary>
    /// Configure les clés et contraintes de la table des partenaires.
    /// </summary>
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(500);
        builder.Property(p => p.LogoUrl).HasMaxLength(500);
    }
}
