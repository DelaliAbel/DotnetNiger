using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

public class PostCategoryConfiguration : IEntityTypeConfiguration<PostCategory>
{
    public void Configure(EntityTypeBuilder<PostCategory> builder)
    {
        builder.HasKey(pc => new { pc.PostId, pc.CategoryId });
        builder.HasOne(pc => pc.Post).WithMany().HasForeignKey(pc => pc.PostId);
        builder.HasOne(pc => pc.Category).WithMany().HasForeignKey(pc => pc.CategoryId);
    }
}
