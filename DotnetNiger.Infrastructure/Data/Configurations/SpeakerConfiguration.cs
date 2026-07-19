using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class SpeakerConfiguration : IEntityTypeConfiguration<Speaker>
{
    public void Configure(EntityTypeBuilder<Speaker> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Bio).HasMaxLength(1000);
        builder.Property(s => s.AvatarUrl).HasMaxLength(500);
        builder.HasOne(s => s.Event).WithMany(e => e.Speakers).HasForeignKey(s => s.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}
