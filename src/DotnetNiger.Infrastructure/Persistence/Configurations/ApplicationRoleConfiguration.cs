using DotnetNiger.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="ApplicationRole"/>.
/// </summary>
internal sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("AspNetRoles");

        builder.Property(x => x.Description)
            .HasMaxLength(DotnetNiger.Domain.Common.FieldLimits.Excerpt);
    }
}
