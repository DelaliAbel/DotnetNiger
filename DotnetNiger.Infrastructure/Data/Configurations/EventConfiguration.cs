using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.Slug).IsUnique();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Location).HasMaxLength(200);
        builder.Property(e => e.CoverImageUrl).HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasOne(e => e.Organizer).WithMany().HasForeignKey(e => e.OrganizerId);
    }
}
