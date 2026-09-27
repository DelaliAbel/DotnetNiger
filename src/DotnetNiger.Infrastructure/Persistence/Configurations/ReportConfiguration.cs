using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Moderation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="Report"/>.
/// </summary>
internal sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetType)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.TargetId)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.Details)
            .HasMaxLength(FieldLimits.Body);

        builder.Property(x => x.Status)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.ResolutionNote)
            .HasMaxLength(FieldLimits.Body);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.HandledAtUtc);

        builder.Property(x => x.ReporterId)
            .IsRequired();
        builder.Property(x => x.HandledById);

        // Un membre ne peut pas signaler deux fois le même contenu : sans cet index,
        // la file de modération se remplit de doublons.
        builder.HasIndex(x => new { x.TargetType, x.TargetId, x.ReporterId })
            .IsUnique()
            .HasDatabaseName("IX_Reports_Target_Reporter");

        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc })
            .HasDatabaseName("IX_Reports_Status_CreatedAtUtc");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.HandledById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
