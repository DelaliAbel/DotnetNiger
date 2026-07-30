using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Category.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <summary>
    /// Configure les clés, indexes et contraintes de la table des catégories.
    /// </summary>
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.IconUrl).HasMaxLength(500);
    }
}
