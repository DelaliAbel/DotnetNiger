using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Accusé de mise en file d'un essai de modèle de courriel.</summary>
/// <param name="Cle">Clé du modèle essayé.</param>
/// <param name="Titre">Intitulé lisible du modèle.</param>
/// <param name="Destinataire">Adresse qui recevra le message, toujours celle du compte connecté.</param>
/// <param name="Objet">Objet du message mis en file.</param>
/// <param name="Texte">Version texte brut, pour relire l'envoi sans ouvrir une boîte mail.</param>
public sealed record EmailTestQueuedResponse(
    string Cle,
    string Titre,
    string Destinataire,
    string Objet,
    string Texte);

/// <summary>Message déposé via le formulaire de contact.</summary>
/// <param name="Name">Nom de l'auteur du message.</param>
/// <param name="Email">Adresse à laquelle répondre.</param>
/// <param name="Subject">Objet du message.</param>
/// <param name="Message">Corps du message.</param>
public sealed record CreateContactMessageRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(320)] string Email,
    [Required, StringLength(200, MinimumLength = 2)] string Subject,
    [Required, StringLength(32_000, MinimumLength = 10)] string Message);

/// <summary>Message de contact exposé à l'administration.</summary>
/// <param name="Id">Identifiant du message.</param>
/// <param name="Name">Nom de l'auteur du message.</param>
/// <param name="Email">Adresse de l'auteur.</param>
/// <param name="Subject">Objet du message.</param>
/// <param name="Message">Corps du message.</param>
/// <param name="ReceivedAtUtc">Instant de réception (UTC).</param>
/// <param name="HandledAtUtc">Instant de traitement (UTC), ou <see langword="null"/>.</param>
public sealed record ContactMessageResponse(
    Guid Id,
    string Name,
    string Email,
    string Subject,
    string Message,
    DateTime ReceivedAtUtc,
    DateTime? HandledAtUtc);

/// <summary>Demande d'abonnement à la lettre d'information.</summary>
/// <param name="Email">Adresse à abonner.</param>
public sealed record SubscribeToNewsletterRequest(
    [Required, EmailAddress, StringLength(320)] string Email);

/// <summary>Abonnement exposé à l'administration.</summary>
/// <param name="Id">Identifiant de l'abonnement.</param>
/// <param name="Email">Adresse abonnée.</param>
/// <param name="Status">État de l'abonnement.</param>
/// <param name="RequestedAtUtc">Instant de la demande (UTC).</param>
/// <param name="ConfirmedAtUtc">Instant de la confirmation (UTC), ou <see langword="null"/>.</param>
/// <param name="UnsubscribedAtUtc">Instant du désabonnement (UTC), ou <see langword="null"/>.</param>
public sealed record NewsletterSubscriptionResponse(
    Guid Id,
    string Email,
    NewsletterStatus Status,
    DateTime RequestedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? UnsubscribedAtUtc);


