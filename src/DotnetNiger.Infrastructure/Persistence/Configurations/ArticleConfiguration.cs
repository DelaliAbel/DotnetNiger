using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="Article"/>.
/// </summary>
internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(FieldLimits.Title);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(FieldLimits.Slug);

        builder.Property(x => x.Excerpt)
            .IsRequired()
            .HasMaxLength(FieldLimits.Excerpt);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(FieldLimits.Content);

        builder.Property(x => x.Category)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.Status)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedAtUtc);
        builder.Property(x => x.DeletedById);

        builder.Property(x => x.AuthorId)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.Status, x.PublishedAtUtc })
            .HasDatabaseName("IX_Articles_Status_PublishedAtUtc");

        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_Articles_IsDeleted");

        // Relation déclarée explicitement : sans elle, EF ne relie pas la clé étrangère
        // au membre, faute de propriété de navigation dans le domaine. Restrict car un
        // compte n'est jamais supprimé, seulement désactivé.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.WithSoftDeleteFilter();
    }
}
