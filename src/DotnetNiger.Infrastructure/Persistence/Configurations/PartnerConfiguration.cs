using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Partners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="Partner"/>.
/// </summary>
internal sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(FieldLimits.Name);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(FieldLimits.Slug);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(FieldLimits.Excerpt);

        builder.Property(x => x.WebsiteUrl)
            .IsRequired()
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.Tier)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.IsDeleted)
            .IsRequired();
        builder.Property(x => x.DeletedAtUtc);
        builder.Property(x => x.DeletedById);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.Tier, x.SortOrder })
            .HasDatabaseName("IX_Partners_Tier_SortOrder");

        builder.WithSoftDeleteFilter();
    }
}
