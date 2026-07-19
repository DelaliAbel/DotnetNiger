using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Community.Infrastructure.Configurations;

/// <summary>Configuration EF Core pour l'entité Certificate.</summary>
public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.CertificateUrl).HasMaxLength(500);
        entity.Property(e => e.CertificateType).HasMaxLength(100);
        entity.Property(e => e.Status).HasMaxLength(50);
        entity.HasOne(e => e.Member).WithMany(e => e.Certificates).HasForeignKey(e => e.UserId).HasPrincipalKey(e => e.Id);
        entity.HasIndex(e => new { e.UserId, e.Status });
    }
}
