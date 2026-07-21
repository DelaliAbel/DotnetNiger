using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

public class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Platform).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Url).IsRequired().HasMaxLength(500);
        builder.HasOne(s => s.Member).WithMany().HasForeignKey(s => s.MemberId);
    }
}
