using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="Project"/>.
/// </summary>
internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(FieldLimits.Name);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(FieldLimits.Slug);

        builder.Property(x => x.Summary)
            .IsRequired()
            .HasMaxLength(FieldLimits.Excerpt);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(FieldLimits.Content);

        builder.Property(x => x.RepositoryUrl)
            .IsRequired()
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.DemoUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.Technologies)
            .HasMaxLength(FieldLimits.Technologies);

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

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.Status, x.PublishedAtUtc })
            .HasDatabaseName("IX_Projects_Status_PublishedAtUtc");

        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_Projects_IsDeleted");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.WithSoftDeleteFilter();
    }
}
