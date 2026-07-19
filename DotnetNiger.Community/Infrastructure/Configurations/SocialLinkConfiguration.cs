using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité SocialLink.</summary>
public class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Member).WithMany(e => e.SocialLinks).HasForeignKey(e => e.MemberId);
        entity.Property(e => e.Platform).HasMaxLength(50);
    }
}
