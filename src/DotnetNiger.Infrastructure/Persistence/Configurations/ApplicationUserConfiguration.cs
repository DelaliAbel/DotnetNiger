using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="ApplicationUser"/>.
/// </summary>
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(FieldLimits.Name);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(FieldLimits.Name);

        builder.Property(x => x.Slug)
            .HasMaxLength(FieldLimits.Slug);

        builder.Property(x => x.Bio)
            .HasMaxLength(FieldLimits.Bio);

        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.City)
            .HasMaxLength(FieldLimits.Location);

        builder.Property(x => x.GitHubUsername)
            .HasMaxLength(FieldLimits.Handle);

        builder.Property(x => x.LinkedInUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.XUrl)
            .HasMaxLength(FieldLimits.Url);

        builder.Property(x => x.Technologies)
            .HasMaxLength(FieldLimits.Technologies);

        builder.Property(x => x.IsTeam)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        // Calculée à la volée, jamais stockée.
        builder.Ignore(x => x.FullName);

        // Index non unique : un profil non encore complété n'a pas de slug, et SQL Server
        // considère toutes les valeurs NULL comme égales, ce qui rend un index unique
        // filtré impossible à écrire avec la même syntaxe pour SQLite. L'unicité du slug
        // est donc garantie par la couche service, et non par la base.
        builder.HasIndex(x => x.Slug)
            .HasDatabaseName("IX_AspNetUsers_Slug");

        builder.HasIndex(x => x.GitHubUsername)
            .HasDatabaseName("IX_AspNetUsers_GitHubUsername");
    }
}
