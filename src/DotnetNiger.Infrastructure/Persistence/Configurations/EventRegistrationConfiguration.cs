using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration de l'entité <see cref="EventRegistration"/>.
/// </summary>
internal sealed class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.ToTable("EventRegistrations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .AsEnumText();

        builder.Property(x => x.RegisteredAtUtc)
            .IsRequired();
        builder.Property(x => x.CancelledAtUtc);
        builder.Property(x => x.CheckedInAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
        builder.Property(x => x.UpdatedAtUtc);

        builder.Property(x => x.CommunityEventId)
            .IsRequired();
        builder.Property(x => x.MemberId)
            .IsRequired();

        builder.HasIndex(x => new { x.CommunityEventId, x.MemberId })
            .IsUnique()
            .HasDatabaseName("IX_EventRegistrations_Event_Member");

        builder.HasIndex(x => x.MemberId)
            .HasDatabaseName("IX_EventRegistrations_MemberId");

        builder.HasIndex(x => new { x.CommunityEventId, x.Status })
            .HasDatabaseName("IX_EventRegistrations_Event_Status");

        // Si l'événement disparaît physiquement, ses inscriptions le suivent. Un compte
        // n'étant jamais supprimé mais seulement désactivé, la cascade sur le membre est
        // une sécurité Filou : elle empêche une inscription orpheline.
        builder.HasOne<CommunityEvent>()
            .WithMany()
            .HasForeignKey(x => x.CommunityEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
