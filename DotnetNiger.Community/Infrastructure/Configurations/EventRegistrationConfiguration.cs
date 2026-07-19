using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité EventRegistration.</summary>
public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Event).WithMany(e => e.Registrations).HasForeignKey(e => e.EventId);
        entity.Navigation(e => e.Event).IsRequired(false);
        entity.HasIndex(e => new { e.EventId, e.UserId }).IsUnique();
        entity.Property(e => e.RegistrationStatus).HasMaxLength(50);
        entity.Property(e => e.AvatarUrl).HasMaxLength(500);
    }
}
