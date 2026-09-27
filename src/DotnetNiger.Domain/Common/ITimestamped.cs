namespace DotnetNiger.Domain.Common;

/// <summary>
/// Horodatage d'une entité persistée.
/// </summary>
/// <remarks>
/// Toutes les dates sont en UTC. Le provider SQLite gere mal <see cref="DateTimeOffset"/> ;
/// normaliser en <see cref="DateTime"/> UTC garantit un comportement identique sur
/// SQLite et SQL Server.
/// <para>
/// La traçabilité de l'auteur vit dans les clés étrangères métier (<c>AuthorId</c>,
/// <c>CreatedById</c>, <c>ReporterId</c>...) et non dans cette interface, afin d'éviter
/// de dupliquer la même information.
/// </para>
/// </remarks>
public interface ITimestamped
{
    /// <summary>
    /// Obtient la date de création (UTC).
    /// </summary>
    DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Obtient la date de dernière modification (UTC), ou <see langword="null"/> si jamais modifiée.
    /// </summary>
    DateTime? UpdatedAtUtc { get; }
}
