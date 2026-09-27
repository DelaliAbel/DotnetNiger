using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="Comment"/>.
/// </summary>
internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // La contrainte est posée au niveau de la base, pas seulement dans le domaine :
        // elle interdit qu'un commentaire reste sans cible, y compris en cas d'écriture
        // directe en SQL. La syntaxe à crochets est acceptée par SQL Server et SQLite.
        builder.ToTable(
            "Comments",
            table => table.HasCheckConstraint(
                "CK_Comments_ExactlyOneTarget",
                "([ArticleId] IS NOT NULL AND [CommunityEventId] IS NULL) OR ([ArticleId] IS NULL AND [CommunityEventId] IS NOT NULL)"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(FieldLimits.Body);

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

        builder.Property(x => x.AuthorId)
            .IsRequired();

        builder.HasIndex(x => new { x.ArticleId, x.Status, x.CreatedAtUtc })
            .HasDatabaseName("IX_Comments_Article_Status_CreatedAtUtc");

        builder.HasIndex(x => new { x.CommunityEventId, x.Status, x.CreatedAtUtc })
            .HasDatabaseName("IX_Comments_Event_Status_CreatedAtUtc");

        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_Comments_IsDeleted");

        builder.HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CommunityEvent)
            .WithMany()
            .HasForeignKey(x => x.CommunityEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ParentComment)
            .WithMany()
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.WithSoftDeleteFilter();
    }
}
