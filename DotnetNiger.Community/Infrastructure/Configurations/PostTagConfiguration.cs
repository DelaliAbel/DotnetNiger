using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité PostTag.</summary>
public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> entity)
    {
        entity.HasKey(e => new { e.PostId, e.TagId });
        entity.HasOne(e => e.Post).WithMany(e => e.PostTags).HasForeignKey(e => e.PostId);
        entity.HasOne(e => e.Tag).WithMany(e => e.PostTags).HasForeignKey(e => e.TagId);
    }
}
