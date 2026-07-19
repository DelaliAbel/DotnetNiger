using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Speaker.</summary>
public class SpeakerConfiguration : IEntityTypeConfiguration<Speaker>
{
    public void Configure(EntityTypeBuilder<Speaker> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Event).WithMany(e => e.Speakers).HasForeignKey(e => e.EventId);
        entity.Property(e => e.Name).HasMaxLength(200);
        entity.Property(e => e.Role).HasMaxLength(100);
        entity.Property(e => e.AvatarUrl).HasMaxLength(500);
    }
}
