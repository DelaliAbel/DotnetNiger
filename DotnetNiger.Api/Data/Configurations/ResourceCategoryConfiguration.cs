using DotnetNiger.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité ResourceCategory.
/// </summary>
public class ResourceCategoryConfiguration : IEntityTypeConfiguration<ResourceCategory>
{
    /// <summary>
    /// Configure la clé composite et les relations de la table de liaison ressources-catégories.
    /// </summary>
    public void Configure(EntityTypeBuilder<ResourceCategory> builder)
    {
        builder.HasKey(rc => new { rc.ResourceId, rc.CategoryId });
        builder.HasOne(rc => rc.Resource).WithMany().HasForeignKey(rc => rc.ResourceId);
        builder.HasOne(rc => rc.Category).WithMany().HasForeignKey(rc => rc.CategoryId);
    }
}
