using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Comment.
/// </summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    /// <summary>
    /// Configure les clés, relations et contraintes de la table des commentaires.
    /// </summary>
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired().HasMaxLength(2000);
        builder.HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Event).WithMany(e => e.Comments).HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.ParentComment).WithMany(c => c.Replies).HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
    }
}
