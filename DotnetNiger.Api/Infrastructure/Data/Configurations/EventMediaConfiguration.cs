using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité EventMedia.
/// </summary>
public class EventMediaConfiguration : IEntityTypeConfiguration<EventMedia>
{
    /// <summary>
    /// Configure les clés, relations et contraintes de la table des médias d'événements.
    /// </summary>
    public void Configure(EntityTypeBuilder<EventMedia> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.FileType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Title).HasMaxLength(200);
        builder.HasOne(e => e.Event).WithMany(e => e.Medias).HasForeignKey(e => e.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}
