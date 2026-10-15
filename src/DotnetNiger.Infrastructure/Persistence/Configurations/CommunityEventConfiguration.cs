using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="CommunityEvent"/>.
/// </summary>
internal sealed class CommunityEventConfiguration : IEntityTypeConfiguration<CommunityEvent>
{
    public void Configure(EntityTypeBuilder<CommunityEvent> builder)
    {
        builder.ToTable("CommunityEvents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(FieldLimits.Title);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(FieldLimits.Slug);

        builder.Property(x => x.Summary)
            .IsRequired()
            .HasMaxLength(FieldLimits.Excerpt);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(FieldLimits.Content);

        builder.Property(x => x.Format)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.Location)
            .HasMaxLength(FieldLimits.Location);

        builder.Property(x => x.OnlineUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.Kind)
            .IsRequired()
            .HasMaxLength(FieldLimits.Label);

        builder.Property(x => x.Status)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.IsDeleted)
            .IsRequired();
        builder.Property(x => x.DeletedAtUtc);
        builder.Property(x => x.DeletedById);

        builder.Property(x => x.CreatedById)
            .IsRequired();

        builder.Property(x => x.StartsAtUtc)
            .IsRequired();
        builder.Property(x => x.EndsAtUtc)
            .IsRequired();
        builder.Property(x => x.RegistrationClosesAtUtc);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.Status, x.StartsAtUtc })
            .HasDatabaseName("IX_CommunityEvents_Status_StartsAtUtc");

        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_CommunityEvents_IsDeleted");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.WithSoftDeleteFilter();
    }
}
