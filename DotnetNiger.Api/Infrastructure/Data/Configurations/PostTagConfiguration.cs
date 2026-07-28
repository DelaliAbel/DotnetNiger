using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité PostTag.
/// </summary>
public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    /// <summary>
    /// Configure la clé composite et les relations de la table de liaison articles-tags.
    /// </summary>
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.HasKey(pt => new { pt.PostId, pt.TagId });
        builder.HasOne(pt => pt.Post).WithMany().HasForeignKey(pt => pt.PostId);
        builder.HasOne(pt => pt.Tag).WithMany().HasForeignKey(pt => pt.TagId);
    }
}
