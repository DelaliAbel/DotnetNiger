namespace DotnetNiger.Domain.Common;

/// <summary>
/// Base de toutes les entités du domaine DotnetNiger.
/// </summary>
/// <remarks>
/// Les identifiants sont des GUID version 7 : triables par ordre chronologique, ce qui
/// évite la fragmentation des index que provoque un GUID aléatoire utilisé en clé primaire.
/// </remarks>
public abstract class Entity : IEntity
{
    /// <summary>
    /// Initialise une nouvelle entité avec un identifiant GUID version 7.
    /// </summary>
    protected Entity() => Id = Guid.CreateVersion7();

    /// <inheritdoc />
    public Guid Id { get; protected set; }
}
