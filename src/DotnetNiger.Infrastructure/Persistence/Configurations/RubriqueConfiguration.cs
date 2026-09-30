using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="Rubrique"/>.
/// </summary>
internal sealed class RubriqueConfiguration : IEntityTypeConfiguration<Rubrique>
{
    public void Configure(EntityTypeBuilder<Rubrique> builder)
    {
        builder.ToTable("Rubriques");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nom)
            .IsRequired()
            .HasMaxLength(FieldLimits.Title);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(FieldLimits.Slug);

        builder.Property(x => x.Couleur)
            .IsRequired()
            .HasMaxLength(FieldLimits.Label);

        builder.Property(x => x.Ordre)
            .IsRequired();

        // Le slug est la clé de filtrage : deux rubriques ne peuvent pas le partager,
        // sinon une URL mènerait à l'une des deux, au hasard.
        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
