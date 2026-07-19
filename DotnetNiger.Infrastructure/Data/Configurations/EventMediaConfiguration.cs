using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class EventMediaConfiguration : IEntityTypeConfiguration<EventMedia>
{
    public void Configure(EntityTypeBuilder<EventMedia> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.FileType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Title).HasMaxLength(200);
        builder.HasOne(e => e.Event).WithMany(e => e.Medias).HasForeignKey(e => e.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}
