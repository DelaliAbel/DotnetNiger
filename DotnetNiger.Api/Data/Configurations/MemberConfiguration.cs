using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Member.
/// </summary>
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    /// <summary>
    /// Configure les clés, indexes et relations de la table des membres.
    /// </summary>
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Bio).HasMaxLength(1000);
        builder.Property(m => m.Location).HasMaxLength(100);
        builder.Property(m => m.WebsiteUrl).HasMaxLength(500);
        builder.HasIndex(m => m.UserId).IsUnique();
        builder.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId);
    }
}
