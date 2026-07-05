using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Notification.</summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Message).HasMaxLength(500);
        entity.HasIndex(e => new { e.UserId, e.IsRead });
    }
}
