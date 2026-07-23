using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité SocialLink.
/// </summary>
public class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    /// <summary>
    /// Configure les clés et relations de la table des liens sociaux.
    /// </summary>
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Platform).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Url).IsRequired().HasMaxLength(500);
        builder.HasOne(s => s.Member).WithMany().HasForeignKey(s => s.MemberId);
    }
}
