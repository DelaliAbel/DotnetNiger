using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

public class EventTagConfiguration : IEntityTypeConfiguration<EventTag>
{
    public void Configure(EntityTypeBuilder<EventTag> builder)
    {
        builder.HasKey(et => new { et.EventId, et.TagId });
        builder.HasOne(et => et.Event).WithMany(e => e.EventTags).HasForeignKey(et => et.EventId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(et => et.Tag).WithMany().HasForeignKey(et => et.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
