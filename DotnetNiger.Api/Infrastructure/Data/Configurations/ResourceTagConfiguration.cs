using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité ResourceTag.
/// </summary>
public class ResourceTagConfiguration : IEntityTypeConfiguration<ResourceTag>
{
    /// <summary>
    /// Configure la clé composite et les relations de la table de liaison ressources-tags.
    /// </summary>
    public void Configure(EntityTypeBuilder<ResourceTag> builder)
    {
        builder.HasKey(rt => new { rt.ResourceId, rt.TagId });
        builder.HasOne(rt => rt.Resource).WithMany().HasForeignKey(rt => rt.ResourceId);
        builder.HasOne(rt => rt.Tag).WithMany().HasForeignKey(rt => rt.TagId);
    }
}
