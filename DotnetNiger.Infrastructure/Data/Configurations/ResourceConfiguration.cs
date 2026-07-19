using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Slug).IsRequired().HasMaxLength(200);
        builder.HasIndex(r => r.Slug).IsUnique();
        builder.Property(r => r.Description).IsRequired();
        builder.Property(r => r.DownloadUrl).HasMaxLength(500);
        builder.Property(r => r.ThumbnailUrl).HasMaxLength(500);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasOne(r => r.Author).WithMany().HasForeignKey(r => r.AuthorId);
    }
}
