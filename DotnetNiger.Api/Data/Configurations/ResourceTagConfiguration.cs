using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

public class ResourceTagConfiguration : IEntityTypeConfiguration<ResourceTag>
{
    public void Configure(EntityTypeBuilder<ResourceTag> builder)
    {
        builder.HasKey(rt => new { rt.ResourceId, rt.TagId });
        builder.HasOne(rt => rt.Resource).WithMany().HasForeignKey(rt => rt.ResourceId);
        builder.HasOne(rt => rt.Tag).WithMany().HasForeignKey(rt => rt.TagId);
    }
}
