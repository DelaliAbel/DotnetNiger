using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Comment.</summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.PostId);
        entity.HasIndex(e => e.EventId);
        entity.HasIndex(e => e.ParentCommentId);
        entity.HasOne(e => e.Post).WithMany(e => e.Comments).HasForeignKey(e => e.PostId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(e => e.Event).WithMany(e => e.Comments).HasForeignKey(e => e.EventId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(e => e.ParentComment).WithMany(e => e.Replies).HasForeignKey(e => e.ParentCommentId).OnDelete(DeleteBehavior.NoAction);
    }
}
