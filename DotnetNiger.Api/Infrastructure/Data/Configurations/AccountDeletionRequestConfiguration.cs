using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité AccountDeletionRequest.
/// </summary>
public class AccountDeletionRequestConfiguration : IEntityTypeConfiguration<AccountDeletionRequest>
{
    /// <summary>
    /// Configure les clés, indexes et relations de la table des demandes de suppression.
    /// </summary>
    public void Configure(EntityTypeBuilder<AccountDeletionRequest> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => d.UserId).IsUnique().HasFilter("[IsProcessed] = 0 AND [CancelledAt] IS NULL");
        builder.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId);
    }
}
