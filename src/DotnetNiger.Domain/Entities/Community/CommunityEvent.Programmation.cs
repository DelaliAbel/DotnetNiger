using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Community;

/// <summary>Quand l'événement a lieu et comment on y participe.</summary>
public sealed partial class CommunityEvent
{
    /// <summary>
    /// Obtient la date de début (UTC).
    /// </summary>
    public DateTime StartsAtUtc { get; private set; }

    /// <summary>
    /// Obtient la date de fin (UTC).
    /// </summary>
    public DateTime EndsAtUtc { get; private set; }

    /// <summary>
    /// Obtient le lieu physique, obligatoire hors ligne.
    /// </summary>
    public string? Location { get; private set; }

    /// <summary>
    /// Obtient le lien de participation en ligne, obligatoire en ligne.
    /// </summary>
    public string? OnlineUrl { get; private set; }

    /// <summary>
    /// Obtient le nombre de places, ou <see langword="null"/> si l'événement n'est pas limité.
    /// </summary>
    public int? Capacity { get; private set; }

    /// <summary>
    /// Obtient la date de clôture des inscriptions (UTC), ou <see langword="null"/> si elles restent ouvertes jusqu'au début.
    /// </summary>
    public DateTime? RegistrationClosesAtUtc { get; private set; }

    /// <summary>
    /// Définit le lieu physique.
    /// </summary>
    /// <param name="location">Adresse ou salle, ou <see langword="null"/> si l'événement est purement en ligne.</param>
    /// <remarks>
    /// Seul le lieu est vérifié ici. La cohérence complète du format ne peut pas l'être :
    /// le lieu est posé avant le lien de participation, et exiger ce dernier à ce stade
    /// rendrait tout événement en ligne ou hybride impossible à créer, même lorsque le lien
    /// est fourni deux lignes plus loin.
    /// </remarks>
    public void SetLocation(string? location)
    {
        Location = DomainGuard.OptionalAgainstTooLong(location, FieldLimits.Location, nameof(location));
        EnsureLocationIsSet();
    }

    /// <summary>
    /// Définit le lien de participation en ligne.
    /// </summary>
    /// <param name="onlineUrl">URL absolue, ou <see langword="null"/> si l'événement est purement en personne.</param>
    /// <remarks>
    /// Seul le lien est vérifié ici, pour la même raison que dans <see cref="SetLocation"/> :
    /// le lieu peut ne pas être encore posé.
    /// </remarks>
    public void SetOnlineUrl(string? onlineUrl)
    {
        OnlineUrl = DomainGuard.OptionalAgainstHttpUrl(onlineUrl, nameof(onlineUrl));
        EnsureOnlineUrlIsSet();
    }

    /// <summary>
    /// Fixe le nombre de places.
    /// </summary>
    /// <param name="capacity">Nombre de places, ou <see langword="null"/> pour un événement sans limite.</param>
    public void SetCapacity(int? capacity)
    {
        Capacity = capacity is null ? null : DomainGuard.AgainstNegative(capacity.Value, nameof(capacity));
    }

    /// <summary>
    /// Définit la date de clôture des inscriptions.
    /// </summary>
    /// <param name="closesAtUtc">Date de clôture (UTC), ou <see langword="null"/> pour laisser ouvert.</param>
    public void SetRegistrationClosesAt(DateTime? closesAtUtc) => RegistrationClosesAtUtc = closesAtUtc;

    /// <summary>
    /// Indique si les inscriptions sont encore ouvertes à la date donnée.
    /// </summary>
    /// <param name="confirmedRegistrationsCount">Nombre d'inscriptions confirmées connu à cet instant.</param>
    /// <param name="nowUtc">Date de référence (UTC).</param>
    /// <returns><see langword="true"/> si une nouvelle inscription peut être acceptée.</returns>
    public bool CanRegister(int confirmedRegistrationsCount, DateTime nowUtc)
    {
        if (Status != PublicationStatus.Published || nowUtc >= StartsAtUtc)
        {
            return false;
        }

        if (RegistrationClosesAtUtc is not null && nowUtc >= RegistrationClosesAtUtc)
        {
            return false;
        }

        return Capacity is null || confirmedRegistrationsCount < Capacity.Value;
    }

    /// <summary>
    /// Indique si toutes les places sont attribuées.
    /// </summary>
    /// <param name="confirmedRegistrationsCount">Nombre d'inscriptions confirmées.</param>
    /// <returns><see langword="true"/> si l'événement est complet.</returns>
    public bool IsFull(int confirmedRegistrationsCount)
        => Capacity is not null && confirmedRegistrationsCount >= Capacity.Value;
}
