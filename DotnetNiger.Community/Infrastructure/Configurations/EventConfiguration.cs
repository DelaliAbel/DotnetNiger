using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Event.</summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Slug).IsUnique();
        entity.HasIndex(e => e.StartDate);
        entity.HasIndex(e => e.CreatedBy);
        entity.HasIndex(e => new { e.IsPublished, e.EndDate });
        entity.Property(e => e.Title).HasMaxLength(200);
        entity.Property(e => e.Slug).HasMaxLength(200);
        entity.Property(e => e.EventType).HasMaxLength(50);
        entity.Property(e => e.Location).HasMaxLength(200);
        entity.Property(e => e.Category).HasMaxLength(100);
    }
}
