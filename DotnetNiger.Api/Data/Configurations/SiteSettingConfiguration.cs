using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité SiteSetting.
/// </summary>
public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    /// <summary>
    /// Configure les clés et indexes de la table des paramètres du site.
    /// </summary>
    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.Key).IsUnique();
        builder.Property(s => s.Value).IsRequired().HasMaxLength(2000);
    }
}
