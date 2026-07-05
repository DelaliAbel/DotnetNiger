using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité ResourceCategory.</summary>
public class ResourceCategoryConfiguration : IEntityTypeConfiguration<ResourceCategory>
{
    public void Configure(EntityTypeBuilder<ResourceCategory> entity)
    {
        entity.HasKey(e => new { e.ResourceId, e.CategoryId });
        entity.HasOne(e => e.Resource).WithMany(e => e.ResourceCategories).HasForeignKey(e => e.ResourceId);
        entity.Navigation(e => e.Resource).IsRequired(false);
        entity.HasOne(e => e.Category).WithMany(e => e.ResourceCategories).HasForeignKey(e => e.CategoryId);
    }
}
