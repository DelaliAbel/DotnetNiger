using DotnetNiger.Domain.Common;

namespace DotnetNiger.Application.Abstractions.Persistence;

/// <summary>
/// Unité de travail : point d'accès unique aux dépôts et gardienne de la transaction.
/// </summary>
/// <remarks>
/// Toutes les écritures effectuées via les dépôts restent en attente jusqu'à
/// <see cref="SaveChangesAsync"/>. Les transactions explicites servent aux cas qui doivent
/// annuler l'ensemble, par exemple une inscription à un événement qui doit être refusée si
/// la place restante est insuffisante.
/// <para>
/// À noter : <c>UserManager</c> et <c>SignInManager</c> d'ASP.NET Identity appellent
/// <c>SaveChanges</c> eux-mêmes ; ils n'empruntent pas cette unité de travail.
/// </para>
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Obtient le dépôt en écriture d'une entité.
    /// </summary>
    /// <typeparam name="T">Type de l'entité.</typeparam>
    /// <returns>Dépôt correspondant.</returns>
    IRepository<T> Repository<T>()
        where T : class, IEntity;

    /// <summary>
    /// Obtient le dépôt en écriture d'une entité supprimable logiquement.
    /// </summary>
    /// <typeparam name="T">Type de l'entité.</typeparam>
    /// <returns>Dépôt exposant le retrait.</returns>
    ISoftDeletableRepository<T> SoftDeletableRepository<T>()
        where T : class, ISoftDeletable;

    /// <summary>
    /// Obtient le dépôt en lecture d'une entité.
    /// </summary>
    /// <typeparam name="T">Type de l'entité.</typeparam>
    /// <returns>Dépôt en lecture.</returns>
    IReadRepository<T> ReadRepository<T>()
        where T : class, IEntity;

    /// <summary>
    /// Enregistre toutes les écritures en attente.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Nombre d'entités écrites.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exécute une opération dans une transaction, et la rejoue si la base échoue
    /// temporairement.
    /// </summary>
    /// <remarks>
    /// Dès que la reprise est activée, EF refuse une transaction ouverte à la main : il
    /// faut passer par une stratégie d'exécution. Cette méthode est cette stratégie, et
    /// c'est la seule façon d'ouvrir une transaction sur ce projet sans staples.
    /// <para>
    /// L'opération rejouée doit donc être <em>idempotente</em> : relire l'événement avant
    /// d'écrire l'inscription, plutôt que d'ajouter une ligne à chaque tentative.
    /// </para>
    /// </remarks>
    /// <param name="operation">Travail à exécuter dans la transaction.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche représentant l'exécution.</returns>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
