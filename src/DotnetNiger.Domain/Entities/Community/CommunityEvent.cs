using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Community;

/// <summary>
/// Événement de la communauté : meetup, hackathon, conférence, en ligne ou en personne.
/// </summary>
/// <remarks>
/// La création est réservée aux administrateurs. L'inscription des membres est gérée par
/// l'entité <see cref="EventRegistration"/>. Le nombre de places est un instantané logical :
/// le compte exact des inscriptions confirmées est calculé par la requête, pas stocké ici.
/// </remarks>
public sealed class CommunityEvent : SoftDeletableEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private CommunityEvent()
    {
    }

    /// <summary>
    /// Initialise un nouvel événement à l'état de brouillon.
    /// </summary>
    /// <param name="createdById">Identifiant de l'administrateur créateur.</param>
    /// <param name="title">Titre de l'événement.</param>
    /// <param name="summary">Résumé affiché dans le calendrier.</param>
    /// <param name="description">Description détaillée au format HTML.</param>
    /// <param name="format">Format de l'événement.</param>
    /// <param name="startsAtUtc">Date de début (UTC).</param>
    /// <param name="endsAtUtc">Date de fin (UTC).</param>
    public CommunityEvent(
        Guid createdById,
        string title,
        string summary,
        string description,
        CommunityEventFormat format,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        CreatedById = DomainGuard.AgainstEmpty(createdById, nameof(createdById));
        Title = Required(title, FieldLimits.Title, nameof(title));
        Summary = Required(summary, FieldLimits.Excerpt, nameof(summary));
        Description = Required(description, FieldLimits.Content, nameof(description));
        Format = format;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = EndsAfterStart(endsAtUtc, startsAtUtc);
        Slug = SlugFactory.Create(title);
    }

    /// <summary>
    /// Obtient le titre de l'événement.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le slug d'URL, unique dans la base.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le résumé court.
    /// </summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la description détaillée au format HTML.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le format de l'événement.
    /// </summary>
    public CommunityEventFormat Format { get; private set; }

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
    /// Obtient l'état éditorial.
    /// </summary>
    public PublicationStatus Status { get; private set; }

    /// <summary>
    /// Obtient la date de publication (UTC).
    /// </summary>
    public DateTime? PublishedAtUtc { get; private set; }

    /// <summary>
    /// Obtient l'image de couverture, utilisée pour la vignette de l'événement.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Obtient l'identifiant de l'administrateur créateur.
    /// </summary>
    public Guid CreatedById { get; private set; }

    /// <summary>
    /// Définit le lieu physique.
    /// </summary>
    /// <param name="location">Adresse ou salle, ou <see langword="null"/> si l'événement est purement en ligne.</param>
    public void SetLocation(string? location)
    {
        Location = DomainGuard.OptionalAgainstTooLong(location, FieldLimits.Location, nameof(location));
        EnsureFormatConsistency();
    }

    /// <summary>
    /// Définit le lien de participation en ligne.
    /// </summary>
    /// <param name="onlineUrl">URL absolue, ou <see langword="null"/> si l'événement est purement en personne.</param>
    public void SetOnlineUrl(string? onlineUrl)
    {
        OnlineUrl = DomainGuard.OptionalAgainstHttpUrl(onlineUrl, nameof(onlineUrl));
        EnsureFormatConsistency();
    }

    /// <summary>
    /// Remplace l'image de couverture de l'événement.
    /// </summary>
    /// <param name="coverImageUrl">URL absolue, ou <see langword="null"/> pour retirer l'image.</param>
    /// <remarks>
    /// L'image sert à la vignette de l'événement dans les listes et sur la page d'accueil.
    /// Elle est facultative : un événement sans image reste valide.
    /// </remarks>
    public void SetCoverImage(string? coverImageUrl)
        => CoverImageUrl = DomainGuard.OptionalAgainstHttpUrl(coverImageUrl, nameof(coverImageUrl));

    /// <summary>
    /// Fixe le nombre de places.
    /// </summary>
    /// <param name="capacity">Nombre de places, ou <see langword="null"/> pour un événement sans limite.</param>
    public void SetCapacity(int? capacity)
    {
        Capacity = capacity is null ? null : DomainGuard.AgainstNegative(capacity.Value, nameof(capacity));
    }

    /// <summary>
    /// Met à jour le contenu et le créneau de l'événement.
    /// </summary>
    /// <param name="title">Nouveau titre.</param>
    /// <param name="summary">Nouveau résumé.</param>
    /// <param name="description">Nouvelle description.</param>
    /// <param name="startsAtUtc">Nouvelle date de début (UTC).</param>
    /// <param name="endsAtUtc">Nouvelle date de fin (UTC).</param>
    public void UpdateDetails(
        string title,
        string summary,
        string description,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        var newTitle = Required(title, FieldLimits.Title, nameof(title));

        if (!string.Equals(Title, newTitle, StringComparison.Ordinal))
        {
            Title = newTitle;
            Slug = SlugFactory.Create(newTitle);
        }

        Summary = Required(summary, FieldLimits.Excerpt, nameof(summary));
        Description = Required(description, FieldLimits.Content, nameof(description));
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = EndsAfterStart(endsAtUtc, startsAtUtc);
    }

    /// <summary>
    /// Définit la date de clôture des inscriptions.
    /// </summary>
    /// <param name="closesAtUtc">Date de clôture (UTC), ou <see langword="null"/> pour laisser ouvert.</param>
    public void SetRegistrationClosesAt(DateTime? closesAtUtc) => RegistrationClosesAtUtc = closesAtUtc;

    /// <summary>
    /// Publie l'événement et l'ouvre aux inscriptions.
    /// </summary>
    /// <param name="nowUtc">Date de publication (UTC).</param>
    public void Publish(DateTime nowUtc)
    {
        EnsureFormatConsistency();
        Status = PublicationStatus.Published;
        PublishedAtUtc ??= nowUtc;
    }

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

    /// <summary>
    /// Repasse l'événement à l'état de brouillon, sans le retirer du site s'il était publié.
    /// </summary>
    public void ReturnToDraft() => Status = PublicationStatus.Draft;

    /// <summary>
    /// Archive l'événement une fois terminé.
    /// </summary>
    public void Archive() => Status = PublicationStatus.Archived;

    private void EnsureFormatConsistency()
    {
        var requiresLocation = Format is CommunityEventFormat.InPerson or CommunityEventFormat.Hybrid;
        var requiresOnlineUrl = Format is CommunityEventFormat.Online or CommunityEventFormat.Hybrid;

        if (requiresLocation && string.IsNullOrWhiteSpace(Location))
        {
            throw new DomainException(
                "event_location_required",
                "Un événement en personne ou hybride doit indiquer un lieu.");
        }

        if (requiresOnlineUrl && string.IsNullOrWhiteSpace(OnlineUrl))
        {
            throw new DomainException(
                "event_online_url_required",
                "Un événement en ligne ou hybride doit indiquer un lien de participation.");
        }
    }

    private static DateTime EndsAfterStart(DateTime endsAtUtc, DateTime startsAtUtc)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            throw new DomainException(
                "event_invalid_schedule",
                "La date de fin doit être strictement postérieure à la date de début.");
        }

        return endsAtUtc;
    }

    private static string Required(string value, int maxLength, string field)
        => DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(value, field),
            maxLength,
            field);
}
