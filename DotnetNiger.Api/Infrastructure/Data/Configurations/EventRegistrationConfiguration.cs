using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité EventRegistration.
/// </summary>
public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    /// <summary>
    /// Configure les clés et relations de la table des inscriptions aux événements.
    /// </summary>
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Event)
               .WithMany(e => e.Registrations)
               .HasForeignKey(e => e.EventId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
