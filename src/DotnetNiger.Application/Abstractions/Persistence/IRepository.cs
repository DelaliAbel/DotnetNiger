using DotnetNiger.Domain.Common;

namespace DotnetNiger.Application.Abstractions.Persistence;

/// <summary>
/// Accès en écriture à une entité du domaine.
/// </summary>
/// <typeparam name="T">Type de l'entité.</typeparam>
/// <remarks>
/// Les modifications restent en mémoire jusqu'à l'appel de
/// <see cref="IUnitOfWork.SaveChangesAsync"/>, ce qui permet de regrouper plusieurs
/// entités dans une seule transaction.
/// <para>
/// Ce contrat n'expose volontairement pas de suppression physique : le retrait d'un contenu
/// passe par <see cref="ISoftDeletableRepository{T}.Retire"/>, et les lignes correspondantes
/// restent en base pour préserver l'historique de modération.
/// </para>
/// </remarks>
public interface IRepository<T>
    where T : class, IEntity
{
    /// <summary>
    /// Récupère une entité par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'entité.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'entité, ou <see langword="null"/> si elle n'existe pas ou est retirée.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indique si une entité existe et n'est pas retirée.
    /// </summary>
    /// <param name="id">Identifiant de l'entité.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si l'entité existe.</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inscrit une nouvelle entité dans le changement tracking.
    /// </summary>
    /// <param name="entity">Entité à ajouter.</param>
    void Add(T entity);

    /// <summary>
    /// Marque une entité existante comme modifiée.
    /// </summary>
    /// <param name="entity">Entité à mettre à jour.</param>
    void Update(T entity);
}
