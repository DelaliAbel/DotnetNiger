using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité SiteSetting.</summary>
public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> entity)
    {
        entity.HasKey(e => e.Key);
        entity.Property(e => e.Key).HasMaxLength(100);
        entity.Property(e => e.Type).HasMaxLength(50);
        entity.Property(e => e.Description).HasMaxLength(500);
    }
}
