using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class ResourceCategoryConfiguration : IEntityTypeConfiguration<ResourceCategory>
{
    public void Configure(EntityTypeBuilder<ResourceCategory> builder)
    {
        builder.HasKey(rc => new { rc.ResourceId, rc.CategoryId });
        builder.HasOne(rc => rc.Resource).WithMany().HasForeignKey(rc => rc.ResourceId);
        builder.HasOne(rc => rc.Category).WithMany().HasForeignKey(rc => rc.CategoryId);
    }
}
