using DotnetNiger.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité ContactMessage.
/// </summary>
public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    /// <summary>
    /// Configure les clés et contraintes de la table des messages de contact.
    /// </summary>
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Subject).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Message).IsRequired().HasMaxLength(5000);
    }
}
