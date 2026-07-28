namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'abonnement à la newsletter.</summary>
public record NewsletterSubscriptionResponse(
    // <summary>Identifiant de l'abonnement.</summary>
    Guid Id,
    // <summary>Adresse e-mail abonnée.</summary>
    string Email,
    // <summary>Nom de l'abonné.</summary>
    string Name,
    // <summary>Indique si l'abonnement est confirmé.</summary>
    bool IsConfirmed,
    // <summary>Date d'abonnement.</summary>
    DateTime SubscribedAt);
