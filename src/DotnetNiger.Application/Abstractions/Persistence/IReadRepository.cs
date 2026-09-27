using DotnetNiger.Domain.Common;

namespace DotnetNiger.Application.Abstractions.Persistence;

/// <summary>
/// Accès en lecture à une entité, sous forme de requête composable.
/// </summary>
/// <typeparam name="T">Type de l'entité.</typeparam>
/// <remarks>
/// Le <see cref="IQueryable{T}"/> permet d'appliquer filtres, tri et projection sans
/// matérialiser d'entité inutile. Il ne doit jamais être exposé tel quel par un
/// contrôleur : la couche de présentation travaille avec des DTO, pas avec des
/// <c>IQueryable</c> sérialisables.
/// </remarks>
public interface IReadRepository<T>
    where T : class, IEntity
{
    /// <summary>
    /// Obtient la source de requête, filtrée sur les entités non retirées lorsque
    /// l'entité est supprimable logiquement.
    /// </summary>
    IQueryable<T> Query { get; }
}
