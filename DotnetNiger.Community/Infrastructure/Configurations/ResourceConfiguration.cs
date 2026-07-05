using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Resource.</summary>
public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Slug).IsUnique();
        entity.HasIndex(e => e.CreatedBy);
        entity.Property(e => e.Title).HasMaxLength(200);
        entity.Property(e => e.Slug).HasMaxLength(200);
        entity.Property(e => e.ResourceType).HasMaxLength(50);
        entity.Property(e => e.Level).HasMaxLength(50);
    }
}
