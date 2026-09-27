using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Common;

/// <summary>
/// Base des entités de contenu qu'un administrateur peut retirer du site sans perdre
/// l'historique : commentaires et signalements liés, dates de publication.
/// </summary>
public abstract class SoftDeletableEntity : TimestampedEntity, ISoftDeletable
{
    /// <inheritdoc />
    public bool IsDeleted { get; protected set; }

    /// <inheritdoc />
    public DateTime? DeletedAtUtc { get; protected set; }

    /// <inheritdoc />
    public Guid? DeletedById { get; protected set; }

    /// <inheritdoc />
    public void SoftDelete(DateTime deletedAtUtc, Guid? deletedById = null)
    {
        if (IsDeleted)
        {
            throw new DomainException(
                "entity_already_deleted",
                $"L'entité {GetType().Name} est déjà retirée.");
        }

        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc;
        DeletedById = deletedById;
    }

    /// <inheritdoc />
    public void Restore()
    {
        if (!IsDeleted)
        {
            throw new DomainException(
                "entity_not_deleted",
                $"L'entité {GetType().Name} n'est pas retirée.");
        }

        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedById = null;
    }
}
