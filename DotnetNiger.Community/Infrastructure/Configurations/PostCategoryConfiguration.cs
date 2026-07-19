using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité PostCategory.</summary>
public class PostCategoryConfiguration : IEntityTypeConfiguration<PostCategory>
{
    public void Configure(EntityTypeBuilder<PostCategory> entity)
    {
        entity.HasKey(e => new { e.PostId, e.CategoryId });
        entity.HasOne(e => e.Post).WithMany(e => e.PostCategories).HasForeignKey(e => e.PostId);
        entity.HasOne(e => e.Category).WithMany(e => e.PostCategories).HasForeignKey(e => e.CategoryId);
    }
}
