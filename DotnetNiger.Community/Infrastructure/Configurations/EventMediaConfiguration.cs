using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité EventMedia.</summary>
public class EventMediaConfiguration : IEntityTypeConfiguration<EventMedia>
{
    public void Configure(EntityTypeBuilder<EventMedia> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Event).WithMany(e => e.Medias).HasForeignKey(e => e.EventId);
        entity.Navigation(e => e.Event).IsRequired(false);
        entity.Property(e => e.Type).HasMaxLength(50);
    }
}
