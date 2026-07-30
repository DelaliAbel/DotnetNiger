using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité MemberSkill.
/// </summary>
public class MemberSkillConfiguration : IEntityTypeConfiguration<MemberSkill>
{
    /// <summary>
    /// Configure les clés et relations de la table des compétences de membres.
    /// </summary>
    public void Configure(EntityTypeBuilder<MemberSkill> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SkillName).IsRequired().HasMaxLength(50);
        builder.HasOne(s => s.Member).WithMany().HasForeignKey(s => s.MemberId);
    }
}
