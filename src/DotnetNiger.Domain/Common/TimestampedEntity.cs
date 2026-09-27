namespace DotnetNiger.Domain.Common;

/// <summary>
/// Base des entités qui portent un horodatage de création et de modification.
/// </summary>
public abstract class TimestampedEntity : Entity, ITimestamped
{
    /// <summary>
    /// Obtient la date de création (UTC). Renseignée automatiquement à la persistance.
    /// </summary>
    public DateTime CreatedAtUtc { get; protected set; }

    /// <summary>
    /// Obtient la date de dernière modification (UTC), ou <see langword="null"/> si jamais modifiée.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; protected set; }
}
