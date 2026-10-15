using DotnetNiger.Domain.Entities.Identity;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>Configuration EF du lien externe d'un membre.</summary>
internal sealed class MemberLinkConfiguration : IEntityTypeConfiguration<MemberLink>
{
    public void Configure(EntityTypeBuilder<MemberLink> builder)
    {
        builder.ToTable("MemberLinks");
        builder.HasKey(link => link.Id);

        builder.Property(link => link.Label)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(link => link.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.HasIndex(link => new { link.MemberId, link.SortOrder });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(link => link.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
