using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class MemberSkillConfiguration : IEntityTypeConfiguration<MemberSkill>
{
    public void Configure(EntityTypeBuilder<MemberSkill> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SkillName).IsRequired().HasMaxLength(50);
        builder.HasOne(s => s.Member).WithMany().HasForeignKey(s => s.MemberId);
    }
}
