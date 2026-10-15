using DotnetNiger.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="ContentImage"/>.
/// </summary>
internal sealed class ContentImageConfiguration : IEntityTypeConfiguration<ContentImage>
{
    public void Configure(EntityTypeBuilder<ContentImage> builder)
    {
        builder.ToTable("ContentImages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TypeContenu)
            .IsRequired()
            .HasMaxLength(100);

        // Les octets ne sont pas contraints en base : la limite de 5 Mo est une règle de
        // domaine, vérifiée à la construction, comme pour les photos de profil.
        builder.Property(x => x.Contenu)
            .IsRequired();
    }
}