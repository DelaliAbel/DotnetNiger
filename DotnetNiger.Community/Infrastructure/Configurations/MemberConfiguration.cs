using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Member.</summary>
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Email);
        entity.HasIndex(e => e.FullName);
        entity.HasIndex(e => e.Country);
        entity.Property(e => e.Email).HasMaxLength(256);
        entity.Property(e => e.FullName).HasMaxLength(100);
        entity.Property(e => e.Roles).HasMaxLength(500);
        entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        entity.Property(e => e.Country).HasMaxLength(100);
        entity.Property(e => e.City).HasMaxLength(100);
    }
}
