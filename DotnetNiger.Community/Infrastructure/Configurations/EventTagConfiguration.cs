using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité EventTag.</summary>
public class EventTagConfiguration : IEntityTypeConfiguration<EventTag>
{
    public void Configure(EntityTypeBuilder<EventTag> entity)
    {
        entity.HasKey(e => new { e.EventId, e.TagId });
        entity.HasOne(e => e.Event).WithMany(e => e.EventTags).HasForeignKey(e => e.EventId);
        entity.Navigation(e => e.Event).IsRequired(false);
        entity.HasOne(e => e.Tag).WithMany(e => e.EventTags).HasForeignKey(e => e.TagId);
    }
}
