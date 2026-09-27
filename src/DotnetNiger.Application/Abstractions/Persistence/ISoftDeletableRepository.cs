using DotnetNiger.Domain.Common;

namespace DotnetNiger.Application.Abstractions.Persistence;

/// <summary>
/// Accès en écriture à une entité supprimable logiquement.
/// </summary>
/// <typeparam name="T">Type de l'entité.</typeparam>
/// <remarks>
/// Le retrait est la seule suppression exposée. Il conserve la ligne afin que les
/// commentaires, signalements et liens existants restent cohérents.
/// </remarks>
public interface ISoftDeletableRepository<T> : IRepository<T>
    where T : class, ISoftDeletable
{
    /// <summary>
    /// Retire l'entité du site sans supprimer la ligne.
    /// </summary>
    /// <param name="entity">Entité à retirer.</param>
    /// <param name="deletedAtUtc">Date de retrait (UTC).</param>
    /// <param name="deletedById">Identifiant de l'administrateur, si connu.</param>
    void Retire(T entity, DateTime deletedAtUtc, Guid? deletedById = null);
}
