using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité ResourceTag.</summary>
public class ResourceTagConfiguration : IEntityTypeConfiguration<ResourceTag>
{
    public void Configure(EntityTypeBuilder<ResourceTag> entity)
    {
        entity.HasKey(e => new { e.ResourceId, e.TagId });
        entity.HasOne(e => e.Resource).WithMany(e => e.ResourceTags).HasForeignKey(e => e.ResourceId);
        entity.Navigation(e => e.Resource).IsRequired(false);
        entity.HasOne(e => e.Tag).WithMany(e => e.ResourceTags).HasForeignKey(e => e.TagId);
    }
}
