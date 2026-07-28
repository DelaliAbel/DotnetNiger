using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Post.
/// </summary>
public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    /// <summary>
    /// Configure les clés, indexes et relations de la table des articles.
    /// </summary>
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(200);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Content).IsRequired();
        builder.Property(p => p.Excerpt).HasMaxLength(500);
        builder.Property(p => p.CoverImageUrl).HasMaxLength(500);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasOne(p => p.Author).WithMany().HasForeignKey(p => p.AuthorId);
    }
}
