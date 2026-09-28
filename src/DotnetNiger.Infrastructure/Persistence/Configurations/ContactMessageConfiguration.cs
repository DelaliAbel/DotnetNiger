using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="ContactMessage"/>.
/// </summary>
internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(FieldLimits.Name);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(FieldLimits.EmailAddress);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(FieldLimits.Subject);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(FieldLimits.Content);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.HandledAtUtc);

        // La boîte de réception se lit du plus récent au plus ancien ; l'index évite de trier
        // toute la table à chaque consultation.
        builder.HasIndex(x => x.ReceivedAtUtc)
            .IsDescending();

        // Un message retiré disparaît des listes, sans que l'application ait à s'en souvenir.
        builder.WithSoftDeleteFilter();
    }
}
