using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="ProfilePhoto"/>.
/// </summary>
internal sealed class ProfilePhotoConfiguration : IEntityTypeConfiguration<ProfilePhoto>
{
    public void Configure(EntityTypeBuilder<ProfilePhoto> builder)
    {
        builder.ToTable("ProfilePhotos");
        builder.HasKey(x => x.Id);

        // Un seul profil photo par membre : la contrainte est en base, pas seulement dans
        // l'écran. Deux lignes pour le même membre signifieraient deux photos actives, et
        // le membre ne pourrait plus savoir laquelle il vient de déposer.
        builder.HasIndex(x => x.MembreId)
            .IsUnique();

        builder.Property(x => x.TypeContenu)
            .IsRequired()
            .HasMaxLength(100);

        // Les octets ne sont pas contraints en base : la limite de 2 Mo est une règle de
        // domaine, vérifiée à la construction. La fixer ici en plus avertedait l'échec tardif
        // sur une ligne déjà écrite.
        builder.Property(x => x.Contenu)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<ProfilePhoto>(x => x.MembreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}