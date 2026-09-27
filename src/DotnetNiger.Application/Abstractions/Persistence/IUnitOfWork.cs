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
    /// Ouvre une transaction explicite.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche représentant l'ouverture de la transaction.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Valide la transaction courante.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche représentant la validation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Annule la transaction courante.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche représentant l'annulation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
