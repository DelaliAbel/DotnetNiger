using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité ContactMessage.</summary>
public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.FullName).HasMaxLength(200);
        entity.Property(e => e.Email).HasMaxLength(200);
        entity.Property(e => e.Subject).HasMaxLength(200);
        entity.Property(e => e.Message).HasMaxLength(2000);
    }
}
