using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="OutboxMessage"/>.
/// </summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kind)
            .IsRequired()
            .HasMaxLength(FieldLimits.Key);

        builder.Property(x => x.Recipient)
            .IsRequired()
            .HasMaxLength(FieldLimits.EmailAddress);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(FieldLimits.Subject);

        builder.Property(x => x.HtmlBody)
            .IsRequired();

        builder.Property(x => x.TextBody)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.LastError)
            .HasMaxLength(FieldLimits.Body);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        // Le relais de fond ne traite que les messages dus, par ordre d'ancienneté. Sans cet
        // index, chaque balayage parcours toute la table, y compris les messages envoyés.
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc })
            .HasDatabaseName("IX_OutboxMessages_Dispatch");

        builder.HasIndex(x => x.BroadcastId)
            .HasDatabaseName("IX_OutboxMessages_BroadcastId");
    }
}
