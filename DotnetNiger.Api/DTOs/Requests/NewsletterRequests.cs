namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête d'abonnement à la newsletter.</summary>
public record SubscribeRequest(
    // <summary>Adresse e-mail de l'abonné.</summary>
    string Email,
    // <summary>Nom de l'abonné.</summary>
    string Name);

/// <summary>Requête de désabonnement de la newsletter.</summary>
public record UnsubscribeRequest(
    // <summary>Adresse e-mail de l'abonné.</summary>
    string Email,
    // <summary>Token de désabonnement.</summary>
    string Token);
