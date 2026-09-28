using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="NewsletterSubscription"/>.
/// </summary>
internal sealed class NewsletterSubscriptionConfiguration : IEntityTypeConfiguration<NewsletterSubscription>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscription> builder)
    {
        builder.ToTable("NewsletterSubscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(FieldLimits.EmailAddress);

        builder.Property(x => x.Status)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(FieldLimits.Key);

        builder.Property(x => x.UnsubscribeKey)
            .IsRequired()
            .HasMaxLength(FieldLimits.Key);

        builder.Property(x => x.RequestedAtUtc)
            .IsRequired();

        builder.Property(x => x.ConfirmedAtUtc);

        builder.Property(x => x.UnsubscribedAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        // Une adresse ne peut avoir qu'un abonnement : sans cela, un même courriel confirmé
        // deux fois ne rendrait qu'un enregistrement destinataire, et l'autre resterait en
        // attente pour toujours.
        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
