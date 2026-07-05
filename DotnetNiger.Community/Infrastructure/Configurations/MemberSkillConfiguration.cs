using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité MemberSkill.</summary>
public class MemberSkillConfiguration : IEntityTypeConfiguration<MemberSkill>
{
    public void Configure(EntityTypeBuilder<MemberSkill> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Member).WithMany(e => e.Skills).HasForeignKey(e => e.MemberId);
        entity.Property(e => e.Name).HasMaxLength(100);
    }
}
