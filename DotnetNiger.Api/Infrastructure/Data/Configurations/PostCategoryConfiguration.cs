using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité PostCategory.
/// </summary>
public class PostCategoryConfiguration : IEntityTypeConfiguration<PostCategory>
{
    /// <summary>
    /// Configure la clé composite et les relations de la table de liaison articles-catégories.
    /// </summary>
    public void Configure(EntityTypeBuilder<PostCategory> builder)
    {
        builder.HasKey(pc => new { pc.PostId, pc.CategoryId });
        builder.HasOne(pc => pc.Post).WithMany().HasForeignKey(pc => pc.PostId);
        builder.HasOne(pc => pc.Category).WithMany().HasForeignKey(pc => pc.CategoryId);
    }
}
