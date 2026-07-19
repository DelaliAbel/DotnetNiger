namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un abonnement à la newsletter.</summary>
public record NewsletterSubscriptionResponse(
    Guid Id, string Email, string Name, bool IsActive, DateTime SubscribedAt);
