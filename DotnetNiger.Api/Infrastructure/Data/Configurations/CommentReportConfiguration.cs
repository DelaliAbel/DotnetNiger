using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité CommentReport.
/// </summary>
public class CommentReportConfiguration : IEntityTypeConfiguration<CommentReport>
{
    /// <summary>
    /// Configure les clés, relations et contraintes de la table des signalements.
    /// </summary>
    public void Configure(EntityTypeBuilder<CommentReport> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasOne(r => r.Comment)
               .WithMany(c => c.Reports)
               .HasForeignKey(r => r.CommentId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => new { r.CommentId, r.UserId }).IsUnique();
    }
}
