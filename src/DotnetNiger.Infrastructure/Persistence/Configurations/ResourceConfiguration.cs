using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="Resource"/>.
/// </summary>
internal sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(FieldLimits.Title);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(FieldLimits.Slug);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(FieldLimits.Excerpt);

        builder.Property(x => x.Content)
            .HasMaxLength(FieldLimits.Content);

        builder.Property(x => x.ExternalUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.Provider)
            .HasMaxLength(FieldLimits.Name);

        builder.Property(x => x.Category)
            .IsRequired()
            .AsEnumText();

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

        builder.Property(x => x.CreatorId)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.Status, x.PublishedAtUtc })
            .HasDatabaseName("IX_Resources_Status_PublishedAtUtc");

        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_Resources_IsDeleted");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.WithSoftDeleteFilter();
    }
}
