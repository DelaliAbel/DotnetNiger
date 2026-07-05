using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Project.</summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Slug).IsUnique();
        entity.HasIndex(e => e.CreatedBy);
        entity.Property(e => e.Title).HasMaxLength(200);
        entity.Property(e => e.Slug).HasMaxLength(200);
        entity.Property(e => e.Technologies).HasMaxLength(500);
        entity.Property(e => e.Status).HasMaxLength(50);
    }
}
