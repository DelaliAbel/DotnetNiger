using DotnetNiger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Data.Configurations;

public class AccountDeletionRequestConfiguration : IEntityTypeConfiguration<AccountDeletionRequest>
{
    public void Configure(EntityTypeBuilder<AccountDeletionRequest> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => d.UserId).IsUnique().HasFilter("[IsProcessed] = 0 AND [CancelledAt] IS NULL");
        builder.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId);
    }
}
