using DotnetNiger.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="ReglageSite"/>.
/// </summary>
internal sealed class ReglageSiteConfiguration : IEntityTypeConfiguration<ReglageSite>
{
    public void Configure(EntityTypeBuilder<ReglageSite> builder)
    {
        builder.ToTable("SiteSettings");

        // La clé du réglage est sa clé primaire : un réglage existe une seule fois, et le
        // remplacer ne doit jamais créer une seconde ligne que personne ne lirait.
        builder.HasKey(x => x.Cle);

        builder.Property(x => x.Cle)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Valeur)
            .HasMaxLength(500);
    }
}