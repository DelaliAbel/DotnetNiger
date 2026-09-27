using DotnetNiger.Application.Abstractions;
using DotnetNiger.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotnetNiger.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Renseigne les horodatages et applique la suppression logique au moment de la sauvegarde.
/// </summary>
/// <remarks>
/// Placé dans un intercepteur plutôt que dans une surcharge de <c>SaveChanges</c> : la
/// règle s'applique donc aussi aux écritures d'ASP.NET Identity, qui appellent
/// <c>SaveChanges</c> sans passer par l'unité de travail.
/// <para>
/// Il constitue également un filet de sécurité : une suppression physique demandée par
/// erreur sur une entité retirable logiquement est convertie en retrait, ce qui préserve
/// l'historique de modération.
/// </para>
/// </remarks>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Initialise l'intercepteur.
    /// </summary>
    /// <param name="timeProvider">Source d'horodatage, injectée pour rendre le comportement testable.</param>
    /// <param name="currentUser">Utilisateur courant, utilisé pour tracer les retraits.</param>
    public AuditSaveChangesInterceptor(TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyRules(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Deleted when entry.Entity is ISoftDeletable softDeletable:
                    // Filet de sécurité : un retrait logique au lieu d'une suppression physique.
                    entry.State = EntityState.Modified;
                    softDeletable.SoftDelete(now, userId);
                    entry.Property(nameof(ITimestamped.UpdatedAtUtc)).CurrentValue = now;
                    break;

                case EntityState.Added when entry.Entity is ITimestamped:
                    if (entry.Property(nameof(ITimestamped.CreatedAtUtc)).CurrentValue is not DateTime created ||
                        created == default)
                    {
                        entry.Property(nameof(ITimestamped.CreatedAtUtc)).CurrentValue = now;
                    }

                    break;

                case EntityState.Modified when entry.Entity is ITimestamped:
                    entry.Property(nameof(ITimestamped.UpdatedAtUtc)).CurrentValue = now;
                    break;

                default:
                    break;
            }
        }
    }
}
