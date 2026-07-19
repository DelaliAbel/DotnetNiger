using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Issuer).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CertificateUrl).HasMaxLength(500);
        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(c => c.Member).WithMany(m => m.Certificates).HasForeignKey(c => c.MemberId).OnDelete(DeleteBehavior.NoAction);
    }
}
