using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Notification.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <summary>
    /// Configure les clés et relations de la table des notifications.
    /// </summary>
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        builder.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId);
    }
}
