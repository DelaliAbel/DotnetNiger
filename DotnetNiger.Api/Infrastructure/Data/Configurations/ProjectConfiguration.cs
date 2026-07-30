using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Project.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <summary>
    /// Configure les clés et contraintes de la table des projets.
    /// </summary>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.RepositoryUrl).HasMaxLength(500);
        builder.Property(p => p.DemoUrl).HasMaxLength(500);
    }
}
