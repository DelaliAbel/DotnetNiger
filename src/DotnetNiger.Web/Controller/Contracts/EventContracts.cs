using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Résumé d'un événement.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si limité.</param>
/// <param name="ConfirmedCount">Inscriptions confirmées.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record EventSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? OnlineUrl,
    int? Capacity,
    int ConfirmedCount,
    string? CoverImageUrl);

/// <summary>Événement tel que l'administration le voit.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si limité.</param>
/// <param name="ConfirmedCount">Inscriptions confirmées.</param>
/// <param name="RegistrationClosesAtUtc">Fin des inscriptions, si planifiée.</param>
/// <param name="Status">État de publication.</param>
/// <param name="Creator">Nom du membre qui a créé l'événement.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record AdminEventSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? OnlineUrl,
    int? Capacity,
    int ConfirmedCount,
    DateTime? RegistrationClosesAtUtc,
    PublicationStatus Status,
    string Creator,
    string? CoverImageUrl);

/// <summary>Demande de création d'un événement.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="Kind">Type de rencontre, parmi ceux du référentiel.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si l'événement est limité.</param>
/// <param name="RegistrationClosesAtUtc">Fin des inscriptions.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
/// <param name="Publish">Publier immédiatement plutôt que garder en brouillon.</param>
public sealed record CreateEventRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Summary,
    [Required, StringLength(32000)] string Description,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    [StringLength(150)] string? Location = null,
    [StringLength(2048)] string? OnlineUrl = null,
    int? Capacity = null,
    DateTime? RegistrationClosesAtUtc = null,
    [StringLength(2048)] string? CoverImageUrl = null,
    bool Publish = false,
    // Le type est ajouté en dernier : un paramètre optionnel ne peut pas précéder un
    // paramètre obligatoire, et les dates doivent le rester.
    [StringLength(50)] string? Kind = null);

/// <summary>Demande de mise à jour d'un événement.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si l'événement est limité.</param>
/// <param name="RegistrationClosesAtUtc">Fin des inscriptions.</param>
/// <param name="CoverImageUrl">Image de couverture, ou <see langword="null"/> pour la retirer.</param>
/// <param name="Kind">Type de rencontre, parmi ceux du référentiel.</param>
public sealed record UpdateEventRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Summary,
    [Required, StringLength(32000)] string Description,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    [StringLength(150)] string? Location = null,
    [StringLength(2048)] string? OnlineUrl = null,
    int? Capacity = null,
    DateTime? RegistrationClosesAtUtc = null,
    [StringLength(2048)] string? CoverImageUrl = null,
    [StringLength(50)] string? Kind = null);

/// <summary>Événement complet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si l'événement est limité.</param>
/// <param name="RegistrationClosesAtUtc">Fin des inscriptions.</param>
/// <param name="Status">État de publication.</param>
/// <param name="ConfirmedCount">Inscriptions confirmées.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record EventResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Description,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? OnlineUrl,
    int? Capacity,
    DateTime? RegistrationClosesAtUtc,
    PublicationStatus Status,
    int ConfirmedCount,
    string? CoverImageUrl);

/// <summary>Inscription à un événement.</summary>
/// <param name="Id">Identifiant de l'inscription.</param>
/// <param name="MemberId">Membre inscrit.</param>
/// <param name="MemberName">Nom du membre inscrit.</param>
/// <param name="Status">État de l'inscription.</param>
/// <param name="RegisteredAtUtc">Date d'inscription.</param>
/// <param name="CancelledAtUtc">Date d'annulation, si annulée.</param>
/// <param name="CheckedInAtUtc">Date de présence, si l'inscription a été honorée.</param>
public sealed record EventRegistrationResponse(
    Guid Id,
    Guid MemberId,
    string MemberName,
    EventRegistrationStatus Status,
    DateTime RegisteredAtUtc,
    DateTime? CancelledAtUtc,
    DateTime? CheckedInAtUtc);

/// <summary>Inscription du membre connecté, vue depuis son tableau de bord.</summary>
/// <param name="Id">Identifiant de l'inscription.</param>
/// <param name="EventId">Identifiant de la rencontre.</param>
/// <param name="EventSlug">Slug de la rencontre.</param>
/// <param name="EventTitle">Titre de la rencontre.</param>
/// <param name="StartsAtUtc">Début de la rencontre.</param>
/// <param name="Status">État de l'inscription.</param>
public sealed record MyEventRegistrationResponse(
    Guid Id,
    Guid EventId,
    string EventSlug,
    string EventTitle,
    DateTime StartsAtUtc,
    EventRegistrationStatus Status);

