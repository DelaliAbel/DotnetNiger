namespace DotnetNiger.Domain.Common;

/// <summary>
/// Entité retirée logiquement du site sans que la ligne soit supprimée.
/// </summary>
/// <remarks>
/// Le retrait conserve la ligne pour que les commentaires, signalements et liens
/// existants restent cohérents. Le filtrage est appliqué par un global query filter
/// EF Core : aucune requête ne peut oublier la condition.
/// <para>
/// Un <c>DELETE</c> physique ne doit pas être utilisé sur ces tables. Un intercepteur
/// de sauvegarde convertit malgré tout toute suppression physique en retrait.
/// </para>
/// </remarks>
public interface ISoftDeletable : ITimestamped, IEntity
{
    /// <summary>
    /// Obtient une valeur indiquant si l'entité est retirée.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Obtient la date de retrait (UTC), ou <see langword="null"/> si l'entité est active.
    /// </summary>
    DateTime? DeletedAtUtc { get; }

    /// <summary>
    /// Obtient l'identifiant de l'administrateur à l'origine du retrait, si connu.
    /// </summary>
    Guid? DeletedById { get; }

    /// <summary>
    /// Retire l'entité du site, sans supprimer la ligne.
    /// </summary>
    /// <param name="deletedAtUtc">Date de retrait (UTC).</param>
    /// <param name="deletedById">Identifiant de l'administrateur, si connu.</param>
    void SoftDelete(DateTime deletedAtUtc, Guid? deletedById = null);

    /// <summary>
    /// Remet l'entité en ligne après un retrait.
    /// </summary>
    void Restore();
}
