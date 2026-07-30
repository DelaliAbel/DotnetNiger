using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Tag.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    /// <summary>
    /// Configure les clés et indexes de la table des tags.
    /// </summary>
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}
